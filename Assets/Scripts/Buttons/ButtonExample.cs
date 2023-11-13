using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample : MonoBehaviour
{
    public Vertex solution;
    //public Vector3Int currentPlayer;
    //public List<Vector3Int> boxLocations = new();
    //public HashSet<int> clicked = new();
    //public HashSet<Vector3Int> playerPositions = new();
    //public HashSet<List<Vector3Int>> boxPos = new();
    //public HashSet<(Vector3Int, bool[,])> visited = new();
    //int clicks = 0;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        //clicked.Add(clicks++);

        //currentPlayer = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        //playerPositions.Add(currentPlayer);

        //boxLocations.Clear();
        //foreach (Vector3Int box in GridManager.Instance.boxLocations) {
        //    boxLocations.Add(box);
        //    boxPos.Add(boxLocations);
        //}
        //Debug.Log("Logged box locations.");

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

        solution = GridManager.Instance.GetComponent<Graph>().BreadthFirstSearch(GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position), GridManager.Instance.boxes);
    }
    private bool[,] EncodeBoxArray(Vector3Int[] boxList)
    {
        int width = GridManager.Instance.boardWidth;
        int height = GridManager.Instance.boardHeight;
        bool[,] boxArray = new bool[width, height];
        boxArray.Initialize();

        foreach (Vector3Int box in boxList) {
            int x = box.x;
            int z = box.z;
            boxArray[x, z] = true;
        }
        return boxArray;
    }
}
