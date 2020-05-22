using System;
using System.Collections.Generic;

namespace AStarDotnet
{
    public interface IGraph<T> 
    {
        bool Accessible(T src, T dest);         // Return whether dest is accessible from src
        IEnumerable<T> GetConnections(T node);  // Get accessible connections from node
        float GetCost(T src, T dest);           // Get cost to connection
        float EstimateCost(T src, T dest);      // Calculate estimated cost to dest
    }

    public class AStarPathfinder<T> 
    {
        private Dictionary<T, NodeData> visited = new Dictionary<T, NodeData>();    // Closed set
        private PriorityQueue<T, float> queue = new PriorityQueue<T, float>();      // Open set

        private float greed = 0.5f;

        // Represents information about a node in the closed set
        private struct NodeData 
        {
            public T CameFrom;  // The neighbouring node with the shortest distance to source
            public float Dist;  // The distance of this node to source

            public NodeData(T cameFrom, float dist)
            {
                CameFrom = cameFrom;
                Dist = dist;
            }
        }

        /// <param name="greediness"> Heuristic weighting, between 0 and 1 </param>
        public AStarPathfinder(float greediness = 0.5f)
        {    
            if (greediness < 0 || greediness > 1) {
                throw new ArgumentException("Greediness must be in the range of 0 to 1");
            }
            greed = greediness;
        }

        /// <summary> Try to find a path from src to dest. Returns null if no path is found. </summary>
        public List<T> FindPath(IGraph<T> graph, T src, T dest)
        {
            if (!graph.Accessible(src, dest)) return null;

            visited.Add(src, new NodeData(src, 0));
            queue.Enqueue(src, 0);

            while (!visited.ContainsKey(dest) && queue.Count > 0)
            {
                T node = queue.Dequeue();
                float dist = visited[node].Dist;

                // Visit all neighbours
                foreach (T neighbour in graph.GetConnections(node))
                {
                    var g = dist + graph.GetCost(node, neighbour); 
                    var closed = visited.TryGetValue(neighbour, out NodeData v);
                    if (!closed || v.Dist > g) visited[neighbour] = new NodeData(node, g);
                    else continue;
                    var f = (1 - greed) * g + greed * graph.EstimateCost(neighbour, dest);
                    queue.Enqueue(neighbour, f);
                }
            }

            var path = ReconstructPath(src, dest);

            queue.Clear();
            visited.Clear();

            return path;
        }

        private List<T> ReconstructPath(T src, T dest)
        {
            List<T> path;
            if (visited.ContainsKey(dest))
            {
                path = new List<T>();
                path.Add(dest);
                T node = dest;

                while (!node.Equals(src))
                {
                    node = visited[node].CameFrom;
                    path.Add(node);
                }

                path.Reverse();
            }
            else path = null;
            return path;
        }
    }

    // Quick and dirty SortedList-based priority queue
    class PriorityQueue<T1, T2> {

        private SortedList<T2, Queue<T1>> data = new SortedList<T2, Queue<T1>>();
        private int count = 0;

        public int Count => count;

        public void Enqueue(T1 item, T2 key)
        {
            Queue<T1> entry;
            data.TryGetValue(key, out entry);
            if (entry == null)
            {
                entry = new Queue<T1>();
                data[key] = entry;
            }
            entry.Enqueue(item);
            count++;
        }

        public T1 Dequeue()
        {
            if (count > 0) {
                var key = data.Keys[0];
                var entry = data[key];
                T1 item = entry.Dequeue();
                count--;
                if (entry.Count == 0) data.Remove(key);
                return item;
            }

            return default(T1);
        }

        public void Clear() {
            data.Clear();
            count = 0;
        } 
    }
}
