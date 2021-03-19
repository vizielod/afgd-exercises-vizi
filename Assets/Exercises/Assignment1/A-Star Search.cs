using System;
using System.Collections.Generic;
using UnityEngine;
using AfGD.Execise3;

namespace AfGD.Assignment1
{
    public static class AStarSearch
    {
        // Exercise 3.3 - Implement A* search
        // Explore the graph and fill the _cameFrom_ dictionairy with data using uniform cost search.
        // Similar to Exercise 3.1 PathFinding.ReconstructPath() will use the data in cameFrom  
        // to reconstruct a path between the start node and end node. 
        //
        // Notes:
        //      Use the data structures used in Exercise 3.1 and 3.2
        //
        private static float HeuristicDistance(Node startPoint, Node endPoint)
        {
            // Heuristic is adjusted to the order of magnitude of the costs
            return (Math.Abs(startPoint.Position.x - endPoint.Position.x) + Math.Abs(startPoint.Position.z - endPoint.Position.z)) * 100;
        }

        public static void Execute(Graph graph, Node startPoint, Node endPoint, Dictionary<Node, Node> cameFrom)
        {
            var frontier = new PriorityQueue<Node>();
            frontier.Enqueue(startPoint, 0);

            var cost_so_far = new Dictionary<Node, float>();
            cost_so_far[startPoint] = 0;

            var neighbours = new List<Node>();

            while (frontier.Count > 0)
            {
                Node current = frontier.Dequeue();

                if (current == endPoint)
                    break;

                neighbours.Clear();
                graph.GetNeighbours(current, neighbours);

                Debug.Log("Visiting: " + current.Name);
                foreach (Node next in neighbours)
                {
                    float new_cost = cost_so_far[current] + graph.GetCost(current, next);
                    if (cost_so_far.ContainsKey(next) && !(cost_so_far[next] > new_cost)) continue;
                    cost_so_far[next] = new_cost;
                    cameFrom[next] = current;
                    float priority = new_cost + HeuristicDistance(next, endPoint);
                    frontier.Enqueue(next, priority);

                }
            }
            //throw new NotImplementedException("Implement A* search algorithm here.");
        }

    }
}