using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample : MonoBehaviour
{
    public Vertex solution;
    //public Vector3Int currentPlayer;
    public List<Vector3Int> boxLocations = new();
    public bool[,] myArray;
    public HashSet<Vertex> visited = new();
    public Vector3Int playerPos = new();
    public int index = 0;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        //clicked.Add(clicks++);

        //currentPlayer = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        //playerPositions.Add(currentPlayer);

        playerPos = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        boxLocations.Clear();
        foreach (Transform box in GridManager.Instance.boxes) {
            boxLocations.Add(GridManager.Instance.GetClosestCell(box.position));
        }
        boxLocations = boxLocations.OrderBy(v => v.x).ToList();
        Vertex solution = new();
        solution.LateConstructor(index, playerPos, boxLocations);
        if (visited.Contains(solution)) {
            Debug.Log("Hashset already contains the game state.");
        }
        else {
            Debug.Log("Game state was added to the hashset.");
            visited.Add(solution);
        }

        //currentPlayer = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        //boxLocations = new Vector3Int[GridManager.Instance.boxLocations.Count];
        //boxLocations = GridManager.Instance.boxLocations.ToArray();
        //(Vector3Int, bool[,]) tuple = (GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position), EncodeBoxArray(boxLocations));
        //if (visited.Contains(tuple)) {
        //    Debug.Log("Game state has already been reached");
        //}
        //else {
        //    visited.Add(tuple);
        //    Debug.Log("Logged a new game state.");
        //}

        //solution = GridManager.Instance.GetComponent<Graph>().BreadthFirstSearch(GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position), GridManager.Instance.boxes);
    }

    
}
