using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex: IEquatable<Vertex>
{
    public List<MoveCommand> myMoves;
    public Vertex myParent;
    public Vector3Int myPlayerLocation;
    public int myIndex;
    public List<Vector3Int> sortedBoxes;

    private Grid grid;

    // Used for the starting vertex.
    public void LateConstructor(int index, Vector3Int position, List<Vector3Int> unsortedBoxes)
    {
        myIndex = index;
        myPlayerLocation = position;
        sortedBoxes = new();
        Vector3Int[] array = new Vector3Int[unsortedBoxes.Count];
        unsortedBoxes.CopyTo(array);
        foreach (var box in unsortedBoxes) {
            sortedBoxes.Add(box);
        }
        sortedBoxes = sortedBoxes.OrderBy(v => v.x).ToList();
        myParent = null;
        myMoves = new();
    }

    // Used for every other vertex.
    public void LateConstructor(int index, Vertex parent, MoveCommand command)
    {
        myIndex = index;
        myPlayerLocation = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);
        myParent = parent;
        sortedBoxes = new();
        Vector3Int[] array = new Vector3Int[GridManager.Instance.boxes.Count];
        foreach (Transform box in GridManager.Instance.boxes) {
            sortedBoxes.Add(GridManager.Instance.GetClosestCell(box.position));
        }
        sortedBoxes = sortedBoxes.OrderBy(v => v.x).ToList();
        myMoves = new();
        MoveCommand[] moveArray = new MoveCommand[parent.myMoves.Count];
        parent.myMoves.CopyTo(moveArray, 0);
        foreach (MoveCommand move in moveArray) {
            myMoves.Add(move);
        }
        myMoves.Add(command);
    }

    public bool Equals(Vertex other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return myPlayerLocation.Equals(other.myPlayerLocation) && sortedBoxes.Count == other.sortedBoxes.Count && sortedBoxes.TrueForAll(other.sortedBoxes.Contains);
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
            foreach (var box in sortedBoxes) {
                hash = hash * 23 + box.GetHashCode();
            }
            return hash;
        }
    }
}
