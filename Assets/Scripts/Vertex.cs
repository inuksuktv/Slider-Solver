using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex: IEquatable<Vertex>
{
    public int Index { get; private set; } = 0;
    public MoveCommand[] Moves { get; private set; }
    public Vertex Parent { get; private set; }
    public GameState State;

    public struct GameState
    {
        public Vector3Int PlayerLocation { get; private set; }
        public List<Vector3Int> BoxLocations { get; private set; }

        public GameState(Vector3Int playerLocation, List<Vector3Int> boxLocations)
        {
            PlayerLocation = playerLocation;

            BoxLocations = boxLocations.OrderBy(v => v.x).ToList();
        }
    }

    // Used for the starting vertex.
    public void LateConstructor(GameState state)
    {
        State = state;

        Moves = new MoveCommand[0];

        Parent = null;
    }

    // Used for every other vertex.
    public void LateConstructor(int index, Vertex parent, MoveCommand command)
    {
        Index = index;
        var playerLocation = GridManager.Instance.GetClosestCell(GridManager.Instance.Player.position);

        var boxes = GridManager.Instance.Boxes;
        List<Vector3Int> boxLocations = new();
        foreach (Transform box in boxes)
        {
            boxLocations.Add(GridManager.Instance.GetClosestCell(box.position));
        }

        State = new GameState(playerLocation, boxLocations);

        Parent = parent;

        Moves = new MoveCommand[parent.Moves.Length + 1];
        parent.Moves.CopyTo(Moves, 0);
        Moves[^1] = command;
    }

    public bool Equals(Vertex other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return (State.PlayerLocation.Equals(other.State.PlayerLocation) &&
            (State.BoxLocations.Count == other.State.BoxLocations.Count) &&
            State.BoxLocations.TrueForAll(other.State.BoxLocations.Contains));
    }

    public override bool Equals(object obj)
    {
        return (ReferenceEquals(this, obj) ||
            (obj is Vertex other) &&
            Equals(other));
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            hash = (23 * hash) + State.PlayerLocation.GetHashCode();
            foreach (var box in State.BoxLocations)
            {
                hash = (23 * hash) + box.GetHashCode();
            }

            return hash;
        }
    }
}
