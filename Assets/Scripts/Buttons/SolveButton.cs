using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SolveButton : MonoBehaviour
{
    public Vertex solution;
    public List<Vector3Int> boxLocations = new();
    public bool[,] myArray;
    public HashSet<Vertex> visited = new();
    public Vector3Int playerPos = new();
    public int index = 0;
    private Graph script;
    bool isSolving = false;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        script = GridManager.Instance.GetComponent<Graph>();
    }

    private void TaskOnClick()
    {
        Debug.Log("Immediate log.");
        if (!isSolving) {
            StartCoroutine(Search());
        }
    }
    private IEnumerator Search()
    {
        isSolving = true;
        Vector3Int playerPosition = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        List<Transform> boxes = GridManager.Instance.boxes;
        Coroutine search = StartCoroutine(script.BreadthFirstSearch(playerPosition, boxes));
        Debug.Log("Waiting for search...");
        yield return search;
        isSolving = false;
    }

}
