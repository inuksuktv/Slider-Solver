using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResetButton : MonoBehaviour
{
    private GameObject _solveButton;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        _solveButton = transform.parent.Find("SolveButton").gameObject;
    }

    private void TaskOnClick()
    {
        DOTween.KillAll();
        if (_solveButton.TryGetComponent<Image>(out var button)) {
            button.color = Color.white;
        }
        GridManager.Instance.RequestReset();
        CommandManager.Instance.UnitIsMoving = false;
    }
}
