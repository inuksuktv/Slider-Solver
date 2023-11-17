using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoveCommand : CommandManager.ICommand
{
    public Vector3Int myFrom, myTo;
    public Transform myUnit;

    public MoveCommand(Vector3Int start, Vector3Int end)
    {
        myFrom = start;
        myTo = end;
        Transform unit = GridManager.Instance.GetTileAtPosition(myFrom).transform.GetChild(0);
        myUnit = unit;
    }

    public void Execute()
    {
        myUnit.DOMove(myTo, 1).OnComplete(() => { GridManager.Instance.UpdateTiles(); });
    }

    public void Undo()
    {
        GridManager.Instance.MoveUnit(myUnit, myFrom);
        GridManager.Instance.UpdateTiles();
    }
}
