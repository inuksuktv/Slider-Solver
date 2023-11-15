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
        //playerPos = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        //boxLocations.Clear();
        //foreach (Transform box in GridManager.Instance.boxes) {
        //    boxLocations.Add(GridManager.Instance.GetClosestCell(box.position));
        //}
        //boxLocations = boxLocations.OrderBy(v => v.x).ToList();
        //Vertex solution = new();
        //solution.LateConstructor(index, playerPos, boxLocations);
        //if (visited.Contains(solution)) {
        //    Debug.Log("Hashset already contains the game state.");
        //}
        //else {
        //    Debug.Log("Game state was added to the hashset.");
        //    visited.Add(solution);
        //}

        solution = GridManager.Instance.GetComponent<Graph>().BreadthFirstSearch(GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position), GridManager.Instance.boxes);
    }

    
}
