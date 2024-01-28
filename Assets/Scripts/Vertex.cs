using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex: IEquatable<Vertex>
{
    public int Index { get; private set; }
    public List<MoveCommand> Moves { get; private set; } = new();
    public List<Vector3Int> Boxes { get; private set; } = new();
    public Vector3Int PlayerLocation { get; private set; } = new();
    public Vertex Parent { get; private set; }

    // Used for the starting vertex.
    public void LateConstructor(Vector3Int playerPos, List<Vector3Int> boxes)
    {
        Index = 0;
        PlayerLocation = playerPos;

        Boxes.Clear();
        foreach (Vector3Int box in boxes) {
            Boxes.Add(box);
        }
        Boxes = Boxes.OrderBy(v => v.x).ToList();

        Parent = null;
        Moves = new();
    }

    // Used for every other vertex.
    public void LateConstructor(int index, Vertex parent, MoveCommand command)
    {
        Index = index;
        PlayerLocation = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);

        Boxes.Clear();
        foreach (Transform box in GridManager.Instance.Boxes) {
            Boxes.Add(GridManager.Instance.GetClosestCell(box.position));
        }
        Boxes = Boxes.OrderBy(v => v.x).ToList();

        Parent = parent;
        Moves = new();
        MoveCommand[] moves = new MoveCommand[parent.Moves.Count];
        parent.Moves.CopyTo(moves, 0);
        foreach (var move in moves) {
            Moves.Add(move);
        }
        Moves.Add(command);
    }

    public bool Equals(Vertex other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return (PlayerLocation.Equals(other.PlayerLocation) &&
            (Boxes.Count == other.Boxes.Count) &&
            Boxes.TrueForAll(other.Boxes.Contains));
    }

    public override bool Equals(object obj)
    {
        return (ReferenceEquals(this, obj) ||
            (obj is Vertex other) &&
            Equals(other));
    }

    public override int GetHashCode()
    {
        unchecked {
            int hash = 17;
            hash = hash * 23 + PlayerLocation.GetHashCode();
            foreach (var box in Boxes) {
                hash = hash * 23 + box.GetHashCode();
            }
            return hash;
        }
    }
}
