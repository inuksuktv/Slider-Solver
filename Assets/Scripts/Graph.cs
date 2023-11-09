using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph
{
    // Number of vertices.
    private int maxVertices;
    private readonly Vertex[] unvisitedNeighbors = new Vertex[4];

    public Graph(int V)
    {
        maxVertices = V;
    }

    // The search starts at vertex s.
    public void BreadthFirstSearch(Vertex s)
    {
        HashSet<(Transform, bool[,])> visited = new HashSet<(Transform, bool[,])>();
        visited.Add(s.GetTuple());

        Queue nextVertex = new();

        nextVertex.Enqueue(s.GetTuple());

        while (nextVertex.Count > 0) {
            unvisitedNeighbors.Initialize();
            var u = nextVertex.Dequeue();

            // Convert from the tuple/vertex to the gameboard.

            // Perform all moves from this game state and get the new game states.

            // If a new game state has not been encountered, mark it visited and enqueue it.
            foreach (Vertex v in unvisitedNeighbors) {
                if (!visited.Contains(v.GetTuple())) {
                    visited.Add(v.GetTuple());
                    nextVertex.Enqueue(v);
                }
            }
        }
    }
}
