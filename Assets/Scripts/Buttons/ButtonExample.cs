using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample : MonoBehaviour
{
    public Vertex solution;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        solution = GridManager.Instance.GetComponent<Graph>().BreadthFirstSearch(player, GridManager.Instance.boxes);
    }
}
