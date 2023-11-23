using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResumeButton : MonoBehaviour
{
    PauseMenu pauseMenuUI;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        if (transform.parent.TryGetComponent<PauseMenu> (out var script)) {
            pauseMenuUI = script;
        }
    }

    public void TaskOnClick()
    {
        pauseMenuUI.TogglePauseMenu();
    }
}
