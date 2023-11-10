using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph: MonoBehaviour
{
    // Number of vertices.
    private int maxVertices;
    public HashSet<(Vector3Int, bool[,])> visited;

    public Graph(int V)
    {
        maxVertices = V;
    }

    // Player and box positions fully describe the mutable game state. That state is our starting vertex 's'.
    public Vertex BreadthFirstSearch(Transform player, List<Transform> boxList)
    {
        Vector3 initialPosition = player.position;
        List<Vector3> initialLocations = new();
        foreach (Transform box in boxList) {
            initialLocations.Add(box.position);
        }
        Vector3Int goal = GridManager.Instance.GetClosestCell(GridManager.Instance.Goal.position);

        // Build a hashset of tuples to represent the mutable game state. This gives an efficient way of comparing new game states to all previous game states.
        visited = new();

        // The 2D array in the tuple represents box locations on the gameboard. Tiles containing a box on the gameboard have the corresponding location in the array set to true.
        Vertex s = new Vertex(GridManager.Instance.GetClosestCell(player.position), boxList);
        visited.Add(s.GetTuple());

        Queue search = new();
        search.Enqueue(s);

        // The first four tiles are being visited correctly. I expect the forward move to be explored next but instead we start from the right move, which was explored most recently.
        // My hashset is perhaps not working as intended. I'm definitely having trouble setting the game state correctly when loading data from a vertex.
        // I should break these problems into smaller pieces and work up to my desired solution step by step.

        int a = 0;
        while (search.Count > 0) {
            // Read the game state from the vertex and move the game pieces.
            Vertex u = (Vertex)search.Dequeue();
            Debug.Log("Vertex " + a + " player position: " + u.myPlayerLocation);
            u.searchIndex = a;
            List<Transform> boxes = u.myBoxes;
            List<Vector3Int> boxLocations = DecodeBoxArray(u.myArray);
            Vector3Int startPosition = u.myPlayerLocation;

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
                player.position = startPosition;
                for (int j = 0; j < boxLocations.Count; j++) {
                    boxes[j].transform.position = boxLocations[j];
                }
                GridManager.Instance.UpdateTiles();

                // If a move is illegal then the command is null and no new vertex is created.
                MoveCommand command = GridManager.Instance.Player.GetComponent<PlayerController>().MoveProcessing(direction);

                if (command != null) {

                    if (command.myUnit.CompareTag("Box") && command.myTo == goal) { continue; }
                    GridManager.Instance.MoveUnit(command.myUnit, command.myTo);
                    GridManager.Instance.UpdateTiles();
                    Debug.Log($"Sending {command.myUnit.name} from " + command.myFrom + " to: " + command.myTo);

                    Vertex v = new(command.myTo, boxes);

                    u.myNeighbors.Add(v);
                    v.myParent = u;
                    if (u.myMoves != null) { v.myMoves = u.myMoves; }
                    v.myMoves.AddLast(command);

                    if (command.myUnit.CompareTag("Player") && command.myTo == goal) {
                        v.searchIndex = a + 1;
                        // This is counting all moces made and not just the solution.
                        Debug.Log("Found a solution at vertex " + v.searchIndex + " after " + v.myMoves.Count + " moves.");
                        return v; }
                }
            }

            // If an unexplored vertex was found, mark it visited and enqueue it. This is the largest search-pruning step.
            foreach (Vertex v in u.myNeighbors) {
                (Vector3Int, bool[,]) tuple = v.GetTuple();
                if (visited.Contains(tuple)) { Debug.Log("Encounted a previously discovered game state."); }
                if (!visited.Contains(tuple)) {
                    visited.Add(tuple);
                    search.Enqueue(v);
                }
            }
            Debug.Log(visited.Count);
            if (a++ > 10000) { Debug.Log("Ended search early."); break; }
        }
        // Debug.Log("Search finished!");
        player.position = initialPosition;
        for (int k = 0; k < initialLocations.Count; k++) {
            boxList[k].position = initialLocations[k];
        }
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
}
