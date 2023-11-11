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
        Debug.Log("Clicked.");
        solution = GridManager.Instance.GetComponent<Graph>().BreadthFirstSearch(GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position), GridManager.Instance.boxes);
    }
}
