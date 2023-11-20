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
    private Graph _graphScript;
    private bool _isSolving = false;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        _graphScript = GridManager.Instance.GetComponent<Graph>();
    }

    private void TaskOnClick()
    {
        if (!_isSolving) {
            Debug.Log("Starting search from current position.");
            StartCoroutine(Search());
        }
    }
    private IEnumerator Search()
    {
        _isSolving = true;
        Vector3Int playerPosition = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        List<Transform> boxes = GridManager.Instance.Boxes;

        Stopwatch sw = new();
        sw.Start();
        Coroutine search = StartCoroutine(_graphScript.BreadthFirstSearch(playerPosition, boxes));
        Debug.Log("Waiting for search...");
        yield return search;
        sw.Stop();
        Debug.Log("Search elapsed time: " + sw.Elapsed);
        _isSolving = false;
    }

}
