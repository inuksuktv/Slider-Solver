using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoveCommand : CommandManager.ICommand
{
    public Vector3Int From { get; private set; }
    public Vector3Int To { get; private set; }
    public Transform Unit { get; private set; }

    public MoveCommand(Vector3Int start, Vector3Int end)
    {
        From = start;
        To = end;
        Transform unit = GridManager.Instance.GetTileAtPosition(From).transform.GetChild(0);
        Unit = unit;
    }

    public void Execute()
    {
        Unit.DOMove(To, 1).OnComplete(() => { GridManager.Instance.UpdateTiles(); });
    }

    public void Undo()
    {
        GridManager.Instance.MoveUnit(Unit, From);
        GridManager.Instance.UpdateTiles();
    }
}
