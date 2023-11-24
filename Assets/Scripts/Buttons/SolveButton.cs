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
    private GameObject _playbackButton;
    public bool IsSolving { get; private set; }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        _graphScript = GridManager.Instance.GetComponent<Graph>();
        _playbackButton = GameObject.Find("Canvas").transform.Find("PlayButton").gameObject;
    }

    private void TaskOnClick()
    {
        if (!GridManager.Instance.SearchIsRunning) {
            Debug.Log("Starting search from current position.");
            StartCoroutine(Search());
        }
    }

    public void HidePlaybackButton()
    {
        _playbackButton.SetActive(false);
    }

    private IEnumerator Search()
    {
        GridManager.Instance.SearchIsRunning = true;
        Vector3Int playerPosition = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        List<Transform> boxes = GridManager.Instance.Boxes;

        _graphScript.FoundSolution = false;

        Stopwatch sw = new();
        sw.Start();
        Coroutine search = StartCoroutine(_graphScript.BreadthFirstSearch(playerPosition, boxes));
        yield return search;
        sw.Stop();
        Debug.Log("Search elapsed time: " + sw.Elapsed);
        if (_graphScript.FoundSolution) {
            if (_playbackButton != null) {
                _playbackButton.SetActive(true);
            }
            if (TryGetComponent<Image>(out var button)) {
                button.color = Color.green;
            }
        }
        else {
            if (TryGetComponent<Image>(out var button)) {
                button.color = Color.red;
            }
        }
        GridManager.Instance.SearchIsRunning = false;
    }
}
