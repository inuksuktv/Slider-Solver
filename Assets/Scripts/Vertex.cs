using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex: IEquatable<Vertex>
{
    public LinkedList<MoveCommand> myMoves;
    public Vertex myParent;
    public Vector3Int myPlayerLocation;
    public int myIndex;
    public List<Vector3Int> boxList;

    // Used for the starting vertex.
    public void LateConstructor(int index, Vector3Int position, List<Vector3Int> sortedBoxes)
    {
        myIndex = index;
        myPlayerLocation = position;
        boxList = new();
        foreach (var box in sortedBoxes) {
            boxList.Add(box);
        }
        boxList = boxList.OrderBy(v => v.x).ToList();
        myParent = null;
        myMoves = new();
    }

    // Used for every other vertex.
    public void LateConstructor(int index, Vector3Int position, Vertex parent, MoveCommand command)
    {
        myIndex = index;
        myPlayerLocation = position;
        myParent = parent;

        myMoves = new();
        MoveCommand[] moveArray = new MoveCommand[parent.myMoves.Count];
        parent.myMoves.CopyTo(moveArray, 0);
        foreach (MoveCommand move in moveArray) {
            myMoves.AddLast(move);
        }
        myMoves.AddLast(command);
    }

    public (Vector3Int, List<Vector3Int>) GetTuple()
    {
        (Vector3Int, List<Vector3Int>) tuple = (myPlayerLocation, boxList);
        return tuple;
    }

    public bool Equals(Vertex other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return myPlayerLocation.Equals(other.myPlayerLocation) && boxList.Count == other.boxList.Count && boxList.TrueForAll(other.boxList.Contains);
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is Vertex other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked {
            int hash = 17;
            hash = hash * 23 + myPlayerLocation.GetHashCode();
            foreach (var box in boxList) {
                hash = hash * 23 + box.GetHashCode();
            }
            return hash;
        }
    }
}
