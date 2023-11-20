using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlaySolution : MonoBehaviour
{
    Graph _graphScript;
    bool _isPlaying = false;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(TaskOnClick);
        _graphScript = GridManager.Instance.GetComponent<Graph>();
    }

    private void TaskOnClick()
    {
        // Play out the solution.
        if (_graphScript.Solution is null) {
            Debug.Log("No solution found.");
        }
        else {
            if (!_isPlaying) {
                GridManager.Instance.UpdateTiles();
                StartCoroutine(DOSolution(_graphScript.Solution.Moves));
            }
        }
    }

    private IEnumerator DOSolution(List<MoveCommand> moves)
    {
        List<Transform> boxes = GridManager.Instance.Boxes;
        foreach (MoveCommand move in moves) {
            _isPlaying = true;
            Transform unit = move.Unit;
            // Detect if we're moving the player or a box.
            if (move.Unit.CompareTag("Player")) {
                unit = GridManager.Instance.Player;
            }
            else if (move.Unit.CompareTag("Box")) {
                foreach (Transform box in boxes) {
                    Vector3Int boxPos = GridManager.Instance.GetClosestCell(box.position);
                    if (boxPos == move.From) {
                        unit = box;
                        break;
                    }
                }
            }
            Tween tween = unit.DOMove(move.To, 1).SetEase(Ease.InOutSine);
            yield return tween.WaitForCompletion();
            _isPlaying = false;
        }
    }
}
