using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
        if (!isSolving) {
            Debug.Log("Starting search from current position.");
            StartCoroutine(Search());
        }
    }
    private IEnumerator Search()
    {
        isSolving = true;
        Vector3Int playerPosition = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        List<Transform> boxes = GridManager.Instance.boxes;

        Stopwatch sw = new();
        sw.Start();
        Coroutine search = StartCoroutine(script.BreadthFirstSearch(playerPosition, boxes));
        Debug.Log("Waiting for search...");
        yield return search;
        sw.Stop();
        Debug.Log("Search elapsed time: " + sw.Elapsed);
        isSolving = false;
    }

}
