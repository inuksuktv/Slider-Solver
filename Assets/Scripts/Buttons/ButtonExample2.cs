using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonExample2 : MonoBehaviour
{
    Transform canvas;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        canvas = GameObject.Find("Canvas").transform;
    }

    private void TaskOnClick()
    {
        //Vector3Int currentPos = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        //if (canvas.Find("SolveButton").TryGetComponent(out ButtonExample script)) {
        //    if (script.playerPositions.Contains(currentPos)) { Debug.Log("Hashset contains " + currentPos); }
        //    else { Debug.Log("Hashset doesn't contain " + currentPos); }
        //}

        // Play out the solution.
        var solveButton = canvas.Find("SolveButton").GetComponent<ButtonExample>();
        List<MoveCommand> solution = solveButton.solution.myMoves;
        if (solution != null) {
            var sequence = DOTween.Sequence();
            foreach (MoveCommand move in solution) {
                sequence.Append(move.myUnit.DOMove(move.myTo, 1).OnComplete(() => { GridManager.Instance.UpdateTiles(); }));
            }
        }




        //ButtonExample script = canvas.Find("SolveButton").GetComponent<ButtonExample>();
        //boxLocations = new();
        //foreach (Transform box in GridManager.Instance.boxes) {
        //    boxLocations.Add(GridManager.Instance.GetClosestCell(box.position));
        //    Debug.Log(box.position);
        //}
        //if (script.boxLocations.Count > 0 && EncodeBoxArray(boxLocations).ToString() == EncodeBoxArray(script.boxLocations).ToString()) {
        //    if (script.boxPos.Contains(EncodeBoxArray(boxLocations).ToString())) { Debug.Log("Encoding/decoding is working, game state detected."); }
        //    else { Debug.Log("This game state has not been detected."); }
        //}
        //else Debug.Log("Button1 doesn't have box locations or Button1 and Button2 data didn't match.");



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

    //private List<Vector3Int> DecodeBoxArray(bool[] boxArray)
    //{
    //    List<Vector3Int> boxList = new List<Vector3Int>();
    //    int width = GridManager.Instance.boardWidth;
    //    int height = GridManager.Instance.boardHeight;

    //    for (int i = 0; i < width * height; i++) {
    //        if (boxArray[i]) {
    //            int x = i % width;
    //            int z = (int)Mathf.Floor(i / width);
    //        }
    //    }
    //    return boxList;
    //}

}
