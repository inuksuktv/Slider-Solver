using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public LinkedList<MoveCommand> myMoves;
    public Vertex myParent;
    public List<Vertex> myNeighbors;
    public Vector3Int myPlayerLocation { get; private set; }
    public List<Transform> myBoxes { get; private set; }
    public bool[,] myArray { get; private set; }
    public int searchIndex;

    public Vertex(Vector3Int position, List<Transform> boxes)
    {
        myPlayerLocation = position;
        myBoxes = boxes;
        myArray = EncodeBoxArray(boxes);
        myMoves = new();
        myNeighbors = new();
        myParent = null;
        searchIndex = 0;
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
