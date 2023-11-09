using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : CommandManager.ICommand
{
    public Vector3Int myFrom, myTo;
    public Transform myUnit;

    public MoveCommand(Vector3Int start, Vector3Int end, Transform unit)
    {
        myFrom = start;
        myTo = end;
        myUnit = unit;
    }

    public void Execute()
    {
        GridManager.Instance.MoveUnit(myUnit, myTo);

        // Send an async task to the animation system?

        // Once it's done, update the tiles in case we moved boxes.
        GridManager.Instance.UpdateTiles();
    }

    public void Undo()
    {
        GridManager.Instance.MoveUnit(myUnit, myFrom);
        GridManager.Instance.UpdateTiles();
    }
}
