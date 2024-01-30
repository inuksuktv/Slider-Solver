using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class PlaySolution : MonoBehaviour
{
    Graph _graphScript;

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
            if (!CommandManager.Instance.UnitIsMoving)
            {
                GridManager.Instance.UpdateTiles();
                var moves = _graphScript.Solution.Moves.ToList<MoveCommand>();
                StartCoroutine(DOSolution(moves));
            }
        }
    }

    private IEnumerator DOSolution(List<MoveCommand> moves)
    {
        CommandManager.Instance.UnitIsMoving = true;

        foreach (MoveCommand move in moves)
        {
            Transform unit = GridManager.Instance.GetTileAtPosition(move.From).transform.GetChild(0);
            
            Tween tween = unit.DOMove(move.To, 1).SetEase(Ease.InOutSine);
            yield return tween.WaitForCompletion();

            GridManager.Instance.UpdateTiles();
        }
        CommandManager.Instance.UnitIsMoving = false;
    }
}
