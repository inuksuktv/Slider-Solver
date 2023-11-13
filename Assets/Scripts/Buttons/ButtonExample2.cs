using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample2 : MonoBehaviour
{
    Transform canvas;
    //int clicks = 0;
    public List<Vector3Int> boxLocations;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        canvas = GameObject.Find("Canvas").transform;
    }

    private void TaskOnClick()
    {
        //if (canvas.Find("SolveButton").TryGetComponent(out ButtonExample script)) {
        //    if (script.clicked.Contains(clicks)) { Debug.Log("Hashset contains " + clicks + "."); }
        //    else { Debug.Log("Hashset doesn't contain " + clicks + "."); }
        //}
        //clicks++;

        //Vector3Int currentPos = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        //if (canvas.Find("SolveButton").TryGetComponent(out ButtonExample script)) {
        //    if (script.playerPositions.Contains(currentPos)) { Debug.Log("Hashset contains " + currentPos); }
        //    else { Debug.Log("Hashset doesn't contain " + currentPos); }
        //}

        //boxLocations.Clear();
        //foreach (Vector3Int box in GridManager.Instance.boxLocations) {
        //    boxLocations.Add(box);
        //}
        //if (canvas.Find("SolveButton").TryGetComponent(out ButtonExample script)) {
        //    if (script.boxPos.Contains(boxLocations)) { Debug.Log("Box locations have been logged."); }
        //    else { Debug.Log("Box locations have not been logged."); }
        //}

        //if (canvas.Find("SolveButton").TryGetComponent(out ButtonExample script)) {
        //    Vector3Int position = script.currentPlayer;
        //    List<Vector3Int> boxes = new();
        //    foreach (var box in script.boxLocations) {
        //        boxes.Add(box);
        //    }
        //    (Vector3Int, bool[,]) tuple = (position, EncodeBoxArray(boxes));
        //    if (!script.visited.Contains(tuple)) {
        //        Debug.Log("Logged a unique vertex.");
        //        script.visited.Add(tuple);
        //    }
        //}

    }

    //private bool[,] EncodeBoxArray(Vector3Int[] boxList)
    //{
    //    int width = GridManager.Instance.boardWidth;
    //    int height = GridManager.Instance.boardHeight;
    //    bool[,] boxArray = new bool[width, height];
    //    boxArray.Initialize();

    //    foreach (Vector3Int box in boxList) {
    //        int x = box.x;
    //        int z = box.z;
    //        boxArray[x, z] = true;
    //    }
    //    return boxArray;
    //}
}
