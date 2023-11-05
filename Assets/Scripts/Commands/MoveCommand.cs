using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : CommandManager.ICommand
{
    private Vector3Int myFrom;
    private Vector3Int myTo;
    private Transform myUnit;

    public MoveCommand(Vector3Int start, Vector3Int end, Transform unit)
    {
        myFrom = start;
        myTo = end;
        myUnit = unit;
    }

    public void Execute()
    {
        GridManager.Instance.MoveUnit(myUnit, myTo);
    }

    public void Undo()
    {

    }
}
