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
    public void SimulatorConstructor(GridSimulator gameBoard, int index, Vertex parent, MoveCommand simulatedMove)
    {
        Index = index;

        var player = gameBoard.Player;
        Vector3Int playerLocation = new(player.X - 2, 0, player.Y - 2);

        List<Vector3Int> boxLocations = new();
        foreach (var box in gameBoard.Boxes)
        {
            boxLocations.Add(new Vector3Int(box.X - 2, 0, box.Y - 2));
        }

        State = new GameState(playerLocation, boxLocations);

        Parent = parent;

        simulatedMove.ConvertSimulatedCoordinatesToGameSpace();

        Moves = new MoveCommand[parent.Moves.Length + 1];
        parent.Moves.CopyTo(Moves, 0);
        Moves[^1] = simulatedMove;
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
