using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex
{
    public List<MoveCommand> myMoves;

    private Transform myPlayer;
    private bool[,] myArray;

    public Vertex(Transform player, bool[,] array)
    {
        myPlayer = player;
        myArray = array;
    }

    public (Transform, bool[,]) GetTuple()
    {
        (Transform, bool[,]) t = (myPlayer, myArray);
        return t;
    }
}
