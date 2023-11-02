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
        // I need to create a static instance of the grid manager so that I can get a reference to the player object.
    }

    public void Undo()
    {

    }
}
