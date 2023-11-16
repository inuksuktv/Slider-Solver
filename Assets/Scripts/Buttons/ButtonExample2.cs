using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;

public class ButtonExample2 : MonoBehaviour
{
    Graph graph;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        graph = GridManager.Instance.GetComponent<Graph>();
    }

    private void TaskOnClick()
    {
        // Play out the solution.
        if (graph.solution is null) {
            Debug.Log("No solution found.");
        }
        else {
            GridManager.Instance.UpdateTiles();
            StartCoroutine(PlaySolution(graph.solution.myMoves));
        }
    }

    private IEnumerator PlaySolution(List<MoveCommand> moves)
    {
        List<Transform> boxes = GridManager.Instance.boxes;
        foreach (MoveCommand move in moves) {
            Transform unit = move.myUnit;
            // Detect if we're moving the player or a box.
            if (move.myUnit.CompareTag("Player")) {
                unit = GridManager.Instance.Player;
                yield return null;
            }
            else if (move.myUnit.CompareTag("Box")) {
                foreach (Transform box in boxes) {
                    Vector3Int boxPos = GridManager.Instance.GetClosestCell(box.position);
                    if (boxPos == move.myFrom) {
                        unit = box;
                        break;
                    }
                }
            }
            Tween tween = unit.DOMove(move.myTo, 1).SetEase(Ease.InOutSine);
            yield return tween.WaitForCompletion();
        }
    }
}
