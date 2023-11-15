using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph : MonoBehaviour
{
    public List<List<Vertex>> adjacency;
    public HashSet<(Vector3Int, List<Vector3Int>)> visited;

    private Vector3Int currentPlayer;
    private List<Vector3Int> boxLocations;
    Vector3Int direction;
    private int maxVertices = 50000;

    public Vertex BreadthFirstSearch(Vector3Int position, List<Transform> boxList)
    {
        float startTime = Time.realtimeSinceStartup;

        // Read the initial game state.
        Vector3Int playerInitial = position;
        List<Vector3Int> boxesInitial = new();
        foreach (Transform box in boxList) {
            boxesInitial.Add(GridManager.Instance.GetClosestCell(box.position));
        }

        // Initialize data structures for the search. I want an adjacency list, a hashset to track visited vertices, a stack of initialized vertices, and a queue.
        adjacency = new List<List<Vertex>>(maxVertices);
        visited = new HashSet<(Vector3Int, List<Vector3Int>)>();
        Stack<Vertex> vertices = new(maxVertices * 5);
        Queue<Vertex> search = new(maxVertices * 3);
        for (int i = 0; i < maxVertices; i++) {
            Vertex z = new();
            vertices.Push(z);

            List<Vertex> list = new(4);
            adjacency.Add(list);
        }

        // Visit the starting vertex, write its data, and queue it for search.
        Vertex start = vertices.Pop();
        int vertexIndex = 0;
        // Refactoring while testing, wheeeee.
        List<Vector3Int> listOfBoxes = new();
        foreach (Transform box in GridManager.Instance.boxes) {
            listOfBoxes.Add(GridManager.Instance.GetClosestCell(box.position));
        }
        listOfBoxes.Sort();
        start.LateConstructor(vertexIndex, playerInitial, listOfBoxes);
        //visited.Add(start.myArray,DecodeBoxArray(start.myArray)); Get a real reference to the position list.
        search.Enqueue(start);

        // Begin the search.
        while (search.Count > 0)
        {
            // Dequeue the next vertex and read the game state. Don't set the game pieces until we're inside the next loop.
            Vertex parentVertex = search.Dequeue();
            currentPlayer = parentVertex.myPlayerLocation;
            boxLocations = parentVertex.boxList;

            // Find adjacent vertices by trying to move in each direction.
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

                // Illegal moves return null. Only legal moves get processed.
                if (command != null) {
                    // Pushing a box onto the goal tile means there's no solution, so don't consider that move.
                    bool isUnsolvable = command.myUnit.CompareTag("Box") && command.myTo == GridManager.Instance.GetClosestCell(GridManager.Instance.Goal.position);
                    if (isUnsolvable) { continue; }

                    // Record the new game vertex and update the adjacency list.
                    Debug.Log("Moving " + command.myUnit.name + " from " + command.myFrom + " to " + command.myTo);
                    GridManager.Instance.MoveUnit(command.myUnit, command.myTo);
                    GridManager.Instance.UpdateTiles();

                    Vertex childVertex = vertices.Pop();
                    vertexIndex++;
                    childVertex.LateConstructor(vertexIndex, command.myTo, parentVertex, command); // There's a bug here if a box is moved. The box location is recorded instead of the player location.
                    adjacency[parentVertex.myIndex].Add(childVertex);

                    // Return if the player arrived at the goal.
                    if (CheckForSolution(childVertex, command)) {
                        float elapsedTime = Time.realtimeSinceStartup - startTime;
                        Debug.Log("Searched for " + elapsedTime + " seconds.");
                        PlaceGamePieces(playerInitial, boxesInitial);
                        return childVertex;
                    }
                }
            }

            // Now the parent vertex's adjacency list has been created. Check if those game states have been reached previously.
            foreach (Vertex neighbour in adjacency[parentVertex.myIndex]) {
                (Vector3Int, List<Vector3Int>) tuple = neighbour.GetTuple();
                // If it's a new game state, enqueue the vertex and mark it visited.
                if (!visited.Contains(neighbour.GetTuple())) {
                    Debug.Log("Added a vertex to the search.");
                    //visited.Add(neighbour.myArray, DecodeBoxArray(neighbour.myArray)); Get an actual reference to the box list.
                    search.Enqueue(neighbour);
                }
            }
            // If we hit the max, break out of the search.
            if (vertexIndex > maxVertices - 5) {
                Debug.Log("Returned with no solution after searching " + maxVertices + " locations.");
                break;
            }
        }
        PlaceGamePieces(playerInitial, boxesInitial);
        float endTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Searched for " + endTime + " seconds.");
        return null;
    }

    private bool CheckForSolution(Vertex childVertex, MoveCommand command)
    {
        Vector3Int goal = GridManager.Instance.GetClosestCell(GridManager.Instance.Goal.position);
        bool isSolved = command.myUnit.CompareTag("Player") && command.myTo == goal;
        if (isSolved) {
            Debug.Log("Found a solution after evaluating " + childVertex.myIndex + " moves.");
            Debug.Log("Solution move count is " + childVertex.myMoves.Count + ".");
            // Read out the solution.
            MoveCommand[] moveArray = new MoveCommand[childVertex.myMoves.Count];
            childVertex.myMoves.CopyTo(moveArray, 0);
            foreach (MoveCommand move in moveArray) {
                Debug.Log(move.myTo);
            }
        }
        return isSolved;
    }
        

    private List<Vector3Int> DecodeBoxArray(bool[,] boxArray)
    {
        List<Vector3Int> boxList = new List<Vector3Int>();
        int width = GridManager.Instance.boardWidth;
        int height = GridManager.Instance.boardHeight;

        for (int z = 0; z < height; z++) {
            for (int x = 0; x < width; x++) {
                if (boxArray[x,z]) {
                    Vector3Int boxPosition = new Vector3Int(x, 0, z);
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
