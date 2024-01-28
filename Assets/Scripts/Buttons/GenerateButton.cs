using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GenerateButton : MonoBehaviour
{
    private GameObject _playbackButton;
    private GameObject _solveButton;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        _playbackButton = transform.parent.Find("PlayButton").gameObject;
        _solveButton = transform.parent.Find("SolveButton").gameObject;
    }

    private void TaskOnClick()
    {
        _playbackButton.SetActive(false);
        if (_solveButton.TryGetComponent<Image>(out var button)) {
            button.color = Color.white;
        }
        DOTween.KillAll();
        GridManager.Instance.DestroyGameboard();
        GridManager.Instance.GenerateGameboard();
        CommandManager.Instance.UnitIsMoving = false;
    }
}
