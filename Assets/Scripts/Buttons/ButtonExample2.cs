using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        ButtonExample solveButton = canvas.Find("SolveButton").GetComponent<ButtonExample>();
        if (solveButton.solution != null) { Debug.Log(solveButton.solution.myMoves.Count); }
    }
}
