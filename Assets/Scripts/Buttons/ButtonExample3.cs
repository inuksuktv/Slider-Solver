using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample3 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    private void TaskOnClick()
    {
        GridManager.Instance.Player.position = GridManager.Instance.startLocation;
        for (int j = 0; j < GridManager.Instance.boxLocations.Count; j++) {
            GridManager.Instance.boxes[j].transform.position = GridManager.Instance.boxLocations[j];
        }
        GridManager.Instance.UpdateTiles();
    }
}
