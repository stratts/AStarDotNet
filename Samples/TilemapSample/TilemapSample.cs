using System;
using System.Collections.Generic;
using System.Numerics;
using AStarDotNet;

namespace TilemapSample
{
    // A basic tilemap, with tiles represented by a 2D array of booleans
    // If a tile is set to true, it can't be navigated to (ie, it contains something)
    class TileMap
    {
        private bool[,] tiles;

        public int Width { get; }
        public int Height { get; }

        public TileMap(int width, int height)
        {
            tiles = new bool[width,height];
            Width = width;
            Height = height;
        }

        public bool ValidTile(int x, int y)
        {
            if (x < 0 || y < 0 || x > Width - 1 || y > Height - 1) return false;
            return true;
        }

        public bool GetTile(int x, int y)
        {
            CheckTile(x, y);
            return tiles[x, y];
        }

        public void SetTile(int x, int y, bool value)
        {
            CheckTile(x, y);
            tiles[x, y] = value;
        }

        public float GetDistance(int srcX, int srcY, int destX, int destY) 
        {
            var dist = new Vector2(destX, destY) - new Vector2(srcX, srcY);
            return dist.Length();
        }

        public IEnumerable<(int x, int y)> GetNeighbours(int x, int y) {
            for (int i = -1; i <= 1; i++) 
            {
                for (int j = -1; j <= 1; j++) 
                {
                    if (i == 0 && j == 0) continue;
                    int xPos = x + i;
                    int yPos = y + j;
                    if (!ValidTile(xPos, yPos)) continue;

                    yield return (xPos, yPos);
                }
            }
        }

        private void CheckTile(int x, int y)
        {
            if (!ValidTile(x, y)) 
                throw new ArgumentOutOfRangeException($"Tile ({x}, {y}) is out of bounds");
        }  
    }

    // Allows the pathfinder to query the map
    // Tiles are accessed using the x and y coordinates, but the pathfinder
    // operates on single objects, so we use a ValueTuple here
    class TileMapGraph : IGraph<(int x, int y)>
    {
        private TileMap map; 

        public TileMapGraph(TileMap map) 
        {
            this.map = map;
        }

        // Return whether dest is accessible from src
        public bool Accessible((int x, int y) src, (int x, int y) dest) 
        {
            return map.GetTile(dest.x, dest.y) == false;
        }

        // Get cost to neighbouring tile
        public float GetCost((int x, int y) src, (int x, int y) dest)
        {
            return map.GetDistance(src.x, src.y, dest.x, dest.y);
        }

        // Get estimated cost to destination from tile
        public float EstimateCost((int x, int y) src, (int x, int y) dest)
        {
            return map.GetDistance(src.x, src.y, dest.x, dest.y);
        }

        // Get neighbouring tiles that can be navigated to
        public IEnumerable<(int x, int y)> GetConnections((int x, int y) node)
        {
            foreach (var tile in map.GetNeighbours(node.x, node.y))
            {
                if (map.GetTile(tile.x, tile.y) == true) continue;
                yield return (tile.x, tile.y);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var map = new TileMap(20, 20);

            PlaceVerticalWall(map, 2, 0, 10);
            PlaceVerticalWall(map, 15, 2, 4);
            PlaceHorizontalWall(map, 5, 5, 10);
            PlaceHorizontalWall(map, 10, 8, 10);
            PlaceHorizontalWall(map, 0, 12, 14);
            PlaceVerticalWall(map, 10, 12, 5);
            PlaceVerticalWall(map, 6, 5, 7);
            
            var pathfinder = new AStarPathfinder<(int, int)>();
            var path = pathfinder.FindPath(new TileMapGraph(map), (0, 0), (3, 14));

            if (path == null) Console.WriteLine("No path could be found :("); 
            else ShowPath(map, path);
        }

        static void PlaceVerticalWall(TileMap map, int x, int y, int length) 
        {
            for (int i = 0; i < length; i++) map.SetTile(x, y + i, true);
        }

        static void PlaceHorizontalWall(TileMap map, int x, int y, int length) 
        {
            for (int i = 0; i < length; i++) map.SetTile(x + i, y, true);
        }

        static void ShowPath(TileMap map, List<(int x, int y)> path) 
        {
            for (int y = 0; y < map.Height; y++) 
            {
                for (int x = 0; x < map.Width; x++) 
                {
                    char c;
                    var p = (x, y);
                    if (path.Contains(p)) {
                        var idx = path.IndexOf(p);
                        if (idx == 0) c = 's';
                        else if (idx == path.Count - 1) c = 'd';
                        else c = '.';
                    }
                    else if (map.GetTile(x, y) == true) c = 'X';
                    else c = ' ';
                    Console.Write(c);
                    Console.Write(' ');
                }

                Console.Write('\n');
            }
        }
    }
}
