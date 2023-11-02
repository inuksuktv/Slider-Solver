using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : CommandManager.ICommand
{
    private Vector3Int myFrom;
    private Vector3Int myTo;

    public MoveCommand(Vector3Int start, Vector3Int end)
    {
        myFrom = start;
        myTo = end;
    }

    public void Execute()
    {

    }

    public void Undo()
    {

    }
}
