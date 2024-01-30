using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridSimulator
{
    int width;
    int height;
    public List<Coordinates> Boxes;
    public Cell[,] GameBoard;
    public Coordinates Player;
    public Coordinates Goal;

    public GridSimulator(Vertex initial, Vector3Int goal)
    {
        width = GridManager.Instance.BoardWidth;
        height = GridManager.Instance.BoardHeight;

        // The 2-D array will represent the gameboard. Shift 4 so all coordinates are >= 0 with extra padding.
        GameBoard = new Cell[width + 4, height + 4];

        ReadBoxLocationsFrom(initial.State);

        Goal = new Coordinates(goal);
        Player = new Coordinates(initial.State.PlayerLocation);
    }

    public struct Coordinates
    {
        public int X;
        public int Y;

        public Coordinates (Vector3Int position)
        {
            // +1 so all tile coordinates are >= 0, +1 for some wall padding.
            X = position.x + 2;
            Y = position.z + 2;
        }

        public Coordinates (int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Coordinates right = new(1, 0);
        public static Coordinates left = new(-1, 0);
        public static Coordinates up = new(0, 1);
        public static Coordinates down = new(0, -1);

        public static Coordinates operator +(Coordinates a, Coordinates b)
            => new(a.X + b.X, a.Y + b.Y);
        public static Coordinates operator -(Coordinates a, Coordinates b)
            => new(a.X - b.X, a.Y - b.Y);
        public static Coordinates operator *(Coordinates a, int b)
            => new(a.X * b, a.Y * b);

        public static bool operator ==(Coordinates? a, Coordinates? b)
        {
            if (a is null)
                return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(Coordinates? a, Coordinates? b)
            => !(a == b);

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return (obj is Coordinates b) && (X == b.X && Y == b.Y);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    public enum Cell
    {
        Slide,
        Box,
        Wall,
        Player,
        Goal
    }

    public void GenerateGameboard()
    {
        PlaceWalls();
        InitializeSlideTiles();
        PlaceBoxes();

        GameBoard[Player.X, Player.Y] = Cell.Player;
        GameBoard[Goal.X, Goal.Y] = Cell.Goal;
    }

    public void SetGameBoard(Vertex.GameState state)
    {
        // This resets all box tiles to slide tiles.
        InitializeSlideTiles();

        Player = new Coordinates(state.PlayerLocation);
        GameBoard[Player.X, Player.Y] = Cell.Player;

        ReadBoxLocationsFrom(state);
        PlaceBoxes();
    }

    public MoveCommand MoveProcessing(Coordinates direction)
    {
        var (origin, target) = SetOriginAndTargetCells(direction);

        // Return null if the move is blocked.
        var testCell = GameBoard[target.X, target.Y];
        if (testCell == Cell.Box || testCell == Cell.Wall)
        {
            return null;
        }

        var destination = FindDestination(origin, direction);

        MoveCommand move = new(origin, destination);
        return move;
    }

    private (Coordinates origin, Coordinates target) SetOriginAndTargetCells(Coordinates direction)
    {
        var originCell = Player;
        var targetCell = Player + direction;

        if (GameBoard[targetCell.X, targetCell.Y] == Cell.Box)
        {
            originCell += direction;
            targetCell += direction;
        }

        return (originCell, targetCell);
    }

    private Coordinates FindDestination(Coordinates origin, Coordinates direction)
    {
        Coordinates destination = new();

        int maxMove = Mathf.Max(width + 1, height + 1);
        for (var i = 1; i <= maxMove; i++)
        {
            Coordinates nextCell = origin + (direction * i);
            Cell testCell = GameBoard[nextCell.X, nextCell.Y];
            var blocksMove = (testCell == Cell.Box) || (testCell == Cell.Wall);
            if (blocksMove)
            {
                destination = nextCell - direction;
                break;
            }
        }
        return destination;
    }

    private void PlaceWalls()
    {
        var totalWidth = GameBoard.GetLength(0);
        var totalHeight = GameBoard.GetLength(1);

        // Place two rows of wall tiles on all sides.
        for (var i = 0; i < totalWidth; i++)
        {
            GameBoard[i, 0] = Cell.Wall;
            GameBoard[i, 1] = Cell.Wall;
            GameBoard[i, totalHeight - 1] = Cell.Wall;
            GameBoard[i, totalHeight - 2] = Cell.Wall;
        }
        for (var j = 0; j < totalHeight; j++)
        {
            GameBoard[0, j] = Cell.Wall;
            GameBoard[1, j] = Cell.Wall;
            GameBoard[totalWidth - 1, j] = Cell.Wall;
            GameBoard[totalWidth - 2, j] = Cell.Wall;
        }
    }

    private void InitializeSlideTiles()
    {
        for (var i = 2; i < width + 2; i++)
        {
            for (var j = 2; j < height + 2; j++)
            {
                GameBoard[i, j] = Cell.Slide;
            }
        }
    }

    public void MoveUnit(MoveCommand move)
    {
        Cell origin = GameBoard[move.Origin.X, move.Origin.Y];
        GameBoard[move.Target.X, move.Target.Y] = origin;

        if (origin == Cell.Player)
        {
            Player = new(move.Target.X, move.Target.Y);
        }
        else if (origin == Cell.Box)
        {
            Boxes.Remove(move.Origin);
            Boxes.Add(move.Target);
        }

        GameBoard[move.Origin.X, move.Origin.Y] = Cell.Slide;
    }

    public void ReadBoxLocationsFrom(Vertex.GameState state)
    {
        Boxes = new();
        foreach (Vector3Int box in state.BoxLocations)
        {
            Boxes.Add(new Coordinates(box));
        }
    }

    private void PlaceBoxes()
    {
        foreach (var box in Boxes)
        {
            GameBoard[box.X, box.Y] = Cell.Box;
        }
    }
}
