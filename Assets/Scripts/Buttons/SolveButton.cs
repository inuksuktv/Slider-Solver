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

        Stopwatch sw = new();
        sw.Start();
        yield return StartCoroutine(_graphScript.BreadthFirstSearch());
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
        var initialGameState = _graphScript.Origin.State;
        GridManager.Instance.SetGameboard(initialGameState);
        GridManager.Instance.SearchIsRunning = false;
    }
}
