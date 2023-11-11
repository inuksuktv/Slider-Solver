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
        if (GridManager.Instance.TryGetComponent<Graph>(out Graph script)) {
            if (script.vertices.TryGetValue(0, out Vertex origin)) {
                Debug.Log(origin.myMoves.Count);
                Debug.Log(origin.myNeighbors.Count);
            }
        }
    }
}
