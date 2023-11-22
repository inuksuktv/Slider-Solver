using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick()
    {
        SliderSettings.Values[0] = TitleGrid.Instance.boardWidth;
        SliderSettings.Values[1] = TitleGrid.Instance.boardHeight;
        SliderSettings.Values[2] = TitleGrid.Instance.boxCount;
        foreach (var setting in SliderSettings.Values) {
            Debug.Log(setting);
        }
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }
}
