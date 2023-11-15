using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph : MonoBehaviour
{
    private List<List<Vertex>> adjacency;
    private HashSet<Vertex> visited;
    private Stack<Vertex> vertices;
    private int vertexIndex;
    private float startTime;
    private PlayerController playerScript;

    private Vector3Int currentPlayer;
    private Vector3Int goal;
    private List<Vector3Int> boxLocations;
    private Vector3Int direction;

    [SerializeField] private int maxVertices;

    public Vertex BreadthFirstSearch(Vector3Int position, List<Transform> boxList)
    {
        startTime = Time.realtimeSinceStartup;
        playerScript = GridManager.Instance.Player.GetComponent<PlayerController>();

        // Store the initial game state.
        Vector3Int playerInitial = position;
        goal = GridManager.Instance.GetClosestCell(GridManager.Instance.Goal.position);
        List<Vector3Int> boxesInitial = new();
        foreach (Transform box in boxList) {
            boxesInitial.Add(GridManager.Instance.GetClosestCell(box.position));
        }

        // Initialize data structures for the search. I use an adjacency list, a hashset to track visited vertices, a stack of initialized vertices, and a queue.
        adjacency = new List<List<Vertex>>(maxVertices);
        visited = new HashSet<Vertex>(maxVertices);
        vertices = new(maxVertices);
        Queue<Vertex> search = new();
        for (int i = 0; i < maxVertices; i++) {
            List<Vertex> list = new(4);
            adjacency.Add(list);

            Vertex v = new();
            vertices.Push(v);
        }

        // Write the starting vertex's data, visit it, and queue it for search.
        Vertex start = vertices.Pop();
        vertexIndex = 0;
        start.LateConstructor(vertexIndex, playerInitial, boxesInitial);
        visited.Add(start);
        search.Enqueue(start);

        // Begin the search.
        while (search.Count > 0) {
            // Dequeue the next vertex and read its game state.
            Vertex parent = search.Dequeue();
            currentPlayer = parent.myPlayerLocation;
            boxLocations = parent.sortedBoxes;

            // Populate the adjacency list for the current vertex by trying to move in each direction.
            TryMoves(parent);

            foreach (Vertex child in adjacency[parent.myIndex]) {
                // Return if the player arrived at the goal.
                if (CheckForSolution(child)) {
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    Debug.Log("Search elapsed time: " + elapsedTime);
                    PlaceGamePieces(playerInitial, boxesInitial);
                    return child;
                }
                // Check if the vertex is in the hashset to see if the game state has been visited previously. If it's new, add it and queue it for search.
                if (!visited.Contains(child)) {
                    visited.Add(child);
                    search.Enqueue(child);
                }
            }
            // If we hit the set maximum search length, break out of the search.
            if (vertexIndex > maxVertices - 5) {
                break;
            }
        }
        PlaceGamePieces(playerInitial, boxesInitial);
        float searchTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Returned with no solution after searching " + vertexIndex + " game states in " + searchTime + " seconds.");
        return null;
    }

    private bool CheckForSolution(Vertex childVertex)
    {
        MoveCommand command = childVertex.myMoves.Last();
        bool isSolved = command.myUnit.CompareTag("Player") && command.myTo == goal;
        if (isSolved) {
            // Read out the solution.
            MoveCommand[] moveArray = new MoveCommand[childVertex.myMoves.Count];
            childVertex.myMoves.CopyTo(moveArray, 0);
            foreach (MoveCommand move in moveArray) {
                Debug.Log(move.myTo);
            }
            Debug.Log("Found a " + childVertex.myMoves.Count + "-move solution after evaluating " + childVertex.myIndex + " game states.");
        }
        return isSolved;
    }
        

    private void PlaceGamePieces(Vector3Int playerFromVertex, List<Vector3Int> boxLocations)
    {
        GridManager.Instance.Player.position = playerFromVertex;
        for (int i = 0; i < boxLocations.Count; i++) {
            GridManager.Instance.boxes[i].position = boxLocations[i];
        }
        GridManager.Instance.UpdateTiles();
    }

    private void TryMoves(Vertex parent)
    {
        for (int moveIndex = 0; moveIndex < 4; moveIndex++) {
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

            MoveCommand command = playerScript.MoveProcessing(direction);

            // Illegal moves return null. Only legal moves get processed.
            if (command != null) {
                // Pushing a box onto the goal tile means there's no solution, so don't consider that move.
                bool isUnsolvable = command.myUnit.CompareTag("Box") && command.myTo == goal;
                if (isUnsolvable) { continue; }

                // Record the new game vertex and update the adjacency list.
                GridManager.Instance.MoveUnit(command.myUnit, command.myTo);
                GridManager.Instance.UpdateTiles();

                Vertex childVertex = vertices.Pop();
                vertexIndex++;
                childVertex.LateConstructor(vertexIndex, parent, command);

                adjacency[parent.myIndex].Add(childVertex);
            }
        }
    }
}
