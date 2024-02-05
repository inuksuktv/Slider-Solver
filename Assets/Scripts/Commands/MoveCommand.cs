using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MoveCommand : CommandManager.ICommand
{
    // Fields for the grid manager to use.
    public Vector3Int From { get; private set; }
    public Vector3Int To { get; private set; }

    // Fields for the grid simulator to use.
    public GridSimulator.Coordinates Origin { get; private set; }
    public GridSimulator.Coordinates Target { get; private set; }

    public MoveCommand(Vector3Int start, Vector3Int end)
    {
        From = start;
        To = end;
    }

    public MoveCommand(GridSimulator.Coordinates origin, GridSimulator.Coordinates target)
    {
        Origin = origin;
        Target = target;
    }

    public void Execute()
    {
        CommandManager.Instance.UnitIsMoving = true;
        Transform activeUnit = GridManager.Instance.GetTileAtPosition(From).transform.GetChild(0);
        activeUnit.DOMove(To, 1).OnComplete(() =>
        {
            GridManager.Instance.UpdateTiles();
            CommandManager.Instance.UnitIsMoving = false;
        });
    }

    public void Undo()
    {
        Transform activeUnit = GridManager.Instance.GetTileAtPosition(From).transform.GetChild(0);
        GridManager.Instance.MoveUnit(activeUnit, From);
        GridManager.Instance.UpdateTiles();
    }

    public void ConvertSimulatedCoordinatesToGameSpace()
    {
        From = new(Origin.X - 2, 0, Origin.Y - 2);

        To = new(Target.X - 2, 0, Target.Y - 2);
    }
}
