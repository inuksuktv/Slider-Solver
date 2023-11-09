using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public LinkedList<MoveCommand> myMoves;
    public Vertex myParent;
    public List<Vertex> myNeighbors = new();
    public Transform myPlayer { get; private set; }
    public List<Transform> myBoxes { get; private set; }
    public bool[,] myArray { get; private set; }

    public Vertex(Transform player, List<Transform> boxes)
    {
        myPlayer = player;
        myBoxes = boxes;
        myArray = EncodeBoxArray(boxes);
    }

    public (Transform, bool[,]) GetTuple()
    {
        (Transform, bool[,]) tuple = (myPlayer, myArray);
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
