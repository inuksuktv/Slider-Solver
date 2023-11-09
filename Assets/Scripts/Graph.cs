using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph
{
    // Number of vertices.
    private int maxVertices;

    private Vector3Int moveDirection;

    public Graph(int V)
    {
        maxVertices = V;
    }

    // Player and box positions fully describe the mutable game state. That state is our starting vertex 's'.
    public void BreadthFirstSearch(Transform player, List<Transform> boxList)
    {
        // Build a hashset of tuples to represent the mutable game state. This gives an efficient way of comparing new game states to all previous game states.
        HashSet<(Transform, bool[,])> visited = new();

        // The 2D array in the tuple represents box locations on the gameboard. Tiles containing a box on the gameboard have the corresponding location in the array set to true.
        Vertex s = new Vertex(player, boxList);
        visited.Add(s.GetTuple());

        Queue search = new();
        search.Enqueue(s);

        while (search.Count > 0)
            {
            // Read the game state from the vertex and move the game pieces.
            Vertex u = (Vertex)search.Dequeue();

            player.transform.position = u.myPlayer.transform.position;
            List<Vector3Int> boxLocations = DecodeBoxArray(u.myArray);
            List<Transform> boxes = new();

            // Determine possible moves.
            Vector3Int direction = new();
            for (int i = 0; i < 4; i++) {
                switch (i) {
                    case 0:
                        direction = Vector3Int.forward;
                        break;
                    case 1:
                        direction = Vector3Int.back;
                        break;
                    case 2:
                        direction = Vector3Int.left;
                        break;
                    case 3:
                        direction = Vector3Int.right;
                        break;
                }

                // Set game pieces according to the vertex data.
                player.transform.position = u.myPlayer.transform.position;
                for (int j = 0; j < boxLocations.Count; j++) {
                    boxes[j].transform.position = boxLocations[j];
                }

                // Get the active unit and check that the move is legal.
                PlayerController script = u.myPlayer.GetComponent<PlayerController>();
                MoveCommand command = script.MoveProcessing(direction);

                // If a move is illegal then the command is null and no new vertex is created.
                if (command != null) {
                    GridManager.Instance.MoveUnit(command.myUnit, command.myTo);
                    GridManager.Instance.UpdateTiles();

                    Vertex v = new(script.transform, boxes);

                    u.myNeighbors.Add(v);
                    v.myParent = u;
                    v.myMoves = u.myMoves;
                    v.myMoves.AddLast(command);
                }
            }

            // If an unexplored vertex was found, mark it visited and enqueue it. This is the largest search-pruning step.
            foreach (Vertex v in u.myNeighbors) {
                (Transform, bool[,]) tuple = v.GetTuple();

                if (!visited.Contains(tuple)) {
                    visited.Add(tuple);
                    search.Enqueue(v);
                }
            }
        }
    }

    private List<Vector3Int> DecodeBoxArray(bool[,] boxArray)
    {
        List<Vector3Int> boxList = new List<Vector3Int>();

        for (int x = 0; x < boxArray.GetLength(0); x++) {
            for (int z = 0; z < boxArray.GetLength(1); z++) {
                Vector3Int boxPosition = new(x, 0, z);
                boxList.Add(boxPosition);
            }
        }
        return boxList;
    }
}
