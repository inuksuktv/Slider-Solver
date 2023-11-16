using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ButtonExample : MonoBehaviour
{
    public Vertex solution;
    public List<Vector3Int> boxLocations = new();
    public bool[,] myArray;
    public HashSet<Vertex> visited = new();
    public Vector3Int playerPos = new();
    public int index = 0;
    private Graph script;
    private float startTime = 0;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        script = GridManager.Instance.GetComponent<Graph>();
    }

    private void TaskOnClick()
    {
        Debug.Log("Immediate log.");
        StartCoroutine(Search());
    }
    private IEnumerator Search()
    {
        Vector3Int playerPosition = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        List<Transform> boxes = GridManager.Instance.boxes;
        Coroutine search = StartCoroutine(script.BreadthFirstSearch(playerPosition, boxes));
        Debug.Log("Waiting for search...");
        if (startTime == 0) { startTime = Time.time; }
        yield return search;
        float end = Time.time - startTime;
        Debug.Log("Finished searching after " + end);
    }

}
