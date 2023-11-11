using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph : MonoBehaviour
{
    public List<List<Vertex>> adjacency;
    public HashSet<(Vector3Int, bool[,])> visited;

    private Vector3Int currentPlayer;
    private List<Vector3Int> boxLocations;
    Vector3Int direction;
    private int maxVertices = 1000000;

    public Vertex BreadthFirstSearch(Vector3Int position, List<Transform> boxList)
    {
        // Read the initial game state.
        Vector3Int goal = GridManager.Instance.GetClosestCell(GridManager.Instance.Goal.position);
        Vector3Int playerInitial = position;
        List<Vector3Int> boxesInitial = new();
        foreach (Transform box in boxList) {
            boxesInitial.Add(GridManager.Instance.GetClosestCell(box.position));
        }

        // Initialize data structures for the search. I want an adjacency list, a hashset for visited vertices, a stack of initialized vertices, and a queue.
        adjacency = new List<List<Vertex>>(maxVertices);
        visited = new HashSet<(Vector3Int, bool[,])>(maxVertices);
        Stack<Vertex> vertices = new(maxVertices * 5);
        Queue<Vertex> search = new(maxVertices * 2);
        for (int i = 0; i < maxVertices; i++) {
            Vertex z = new();
            vertices.Push(z);

            List<Vertex> list = new(4);
            adjacency.Add(list);
        }

        // Visit the starting vertex 's', write its data, and queue it for search.
        Vertex s = vertices.Pop();
        int searchIndex = 0;
        s.LateConstructor(searchIndex, playerInitial);
        visited.Add(s.GetTuple());
        search.Enqueue(s);

        while (search.Count > 0)
        {
            // Dequeue the next vertex and read the game state.
            Vertex parentVertex = search.Dequeue();
            currentPlayer = parentVertex.myPlayerLocation;
            boxLocations = DecodeBoxArray(parentVertex.myArray);

            // For each direction, try to move in that direction.
            for (int moveIndex = 0; moveIndex < 4; moveIndex++)
            {
                PlaceGamePieces(currentPlayer, boxLocations);

                switch (moveIndex) {
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

                MoveCommand command = GridManager.Instance.Player.GetComponent<PlayerController>().MoveProcessing(direction);

                // For each legal move from the parent, write the game state to the new vertex and record adjacency.
                if (command != null) {
                    // Pushing a box onto the goal tile means there's no solution, so don't consider that move.
                    bool isUnsolvable = command.myUnit.CompareTag("Box") && command.myTo == goal;
                    if (isUnsolvable) { continue; }

                    // Record the new game vertex and update the adjacency list.
                    Debug.Log("Moving " + command.myUnit.name + " from " + command.myFrom + " to " + command.myTo);
                    GridManager.Instance.MoveUnit(command.myUnit, command.myTo);
                    GridManager.Instance.UpdateTiles();

                    Vertex v = vertices.Pop();
                    searchIndex++;
                    v.LateConstructor(searchIndex, command.myTo, parentVertex, command);
                    Debug.Log("Parent move count: " + parentVertex.myMoves.Count);

                    adjacency[parentVertex.myIndex].Add(v);
                    Debug.Log("Parent vertex adjacency count: " + adjacency[parentVertex.myIndex].Count);

                    // If the player arrived at the goal, return immediately.
                    bool isSolved = command.myUnit.CompareTag("Player") && command.myTo == goal;
                    if (isSolved) {
                        Debug.Log("Found a solution after evaluating " + searchIndex + " moves.");
                        Debug.Log("Solution move count is " + v.myMoves.Count + ".");
                        MoveCommand[] moveArray = new MoveCommand[v.myMoves.Count];
                        v.myMoves.CopyTo(moveArray, 0);
                        foreach (MoveCommand move in moveArray) {
                            Debug.Log(move.myTo);
                        }
                        PlaceGamePieces(playerInitial, boxesInitial);
                        return v;
                    }
                }
            }
            // Now vertex u's adjacency list has been created. Check if those game states have been reached previously.
            foreach (Vertex w in adjacency[parentVertex.myIndex]) {
                (Vector3Int, bool[,]) tuple = w.GetTuple();
                // If it's a new game state, enqueue the vertex and mark it visited.
                if (!visited.Contains(tuple)) {
                    visited.Add(tuple);
                    search.Enqueue(w);
                }
            }
            // If we hit the max, break out of the search.
            if (visited.Count > maxVertices) {
                Debug.Log("Returned with no solution after searching " + maxVertices + " locations.");
                break;
            }
        }
        PlaceGamePieces(playerInitial, boxesInitial);
        return null;
    }

    

    private List<Vector3Int> DecodeBoxArray(bool[,] boxArray)
    {
        List<Vector3Int> boxList = new List<Vector3Int>();

        for (int x = 0; x < boxArray.GetLength(0); x++) {
            for (int z = 0; z < boxArray.GetLength(1); z++) {
                if (boxArray[x, z]) {
                    Vector3Int boxPosition = new(x, 0, z);
                    boxList.Add(boxPosition);
                }
            }
        }
        return boxList;
    }

    private void PlaceGamePieces(Vector3Int playerFromVertex, List<Vector3Int> boxLocations)
    {
        GridManager.Instance.Player.position = playerFromVertex;
        for (int i = 0; i < boxLocations.Count; i++) {
            GridManager.Instance.boxes[i].position = boxLocations[i];
        }
        GridManager.Instance.UpdateTiles();
    }
}
