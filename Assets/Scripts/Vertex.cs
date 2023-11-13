using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public LinkedList<MoveCommand> myMoves;
    public Vertex myParent;
    public Vector3Int myPlayerLocation;
    public bool[,] myArray;
    public int myIndex;

    // Used for the starting vertex.
    public void LateConstructor(int index, Vector3Int position)
    {
        myIndex = index;
        myPlayerLocation = position;
        myArray = EncodeBoxArray(GridManager.Instance.boxes);
        myParent = null;
        myMoves = new();
    }

    // Used for every other vertex.
    public void LateConstructor(int index, Vector3Int position, Vertex parent, MoveCommand command)
    {
        myIndex = index;
        myPlayerLocation = position;
        myArray = EncodeBoxArray(GridManager.Instance.boxes);
        myParent = parent;

        myMoves = new();
        MoveCommand[] moveArray = new MoveCommand[parent.myMoves.Count];
        parent.myMoves.CopyTo(moveArray, 0);
        foreach (MoveCommand move in moveArray) {
            myMoves.AddLast(move);
        }
        myMoves.AddLast(command);
    }

    public (Vector3Int, bool[,]) GetTuple()
    {
        (Vector3Int, bool[,]) tuple = (myPlayerLocation, myArray);
        return tuple;
    }

    private bool[,] EncodeBoxArray(List<Transform> boxList)
    {
        int width = GridManager.Instance.boardWidth;
        int height = GridManager.Instance.boardHeight;
        bool[,] boxArray = new bool[width, height];
        boxArray.Initialize();

        foreach (Transform box in boxList) {
            Vector3Int position = GridManager.Instance.GetClosestCell(box.position);
            int x = position.x;
            int z = position.z;
            boxArray[x, z] = true;
        }
        return boxArray;
    }
}
