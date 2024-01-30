using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Graph : MonoBehaviour
{
    [SerializeField] private int _maxVertices;

    public Vertex Origin { get; private set; }
    public Vertex Solution { get; private set; }
    public bool FoundSolution = false;

    private List<List<Vertex>> _adjacencies;
    private HashSet<Vertex> _visitedVertices;
    private Stack<Vertex> _vertices;
    private Queue<Vertex> _verticesToSearch;

    private int _vertexIndex;
    private int _moveCount;

    private GridManager _gridManager;
    private GridSimulator _gridSimulator;
    //private PlayerController _playerController;
    GridSimulator.Coordinates[] _moves = { GridSimulator.Coordinates.up, GridSimulator.Coordinates.down,
                                            GridSimulator.Coordinates.right, GridSimulator.Coordinates.left};


    private Vector3Int _goal;
    //private readonly Vector3Int[] _moves = { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };

    private void Start()
    {
        // Cache a local reference to reduce verbosity. GridManager is a singleton.
        _gridManager = GridManager.Instance;
    }

    public IEnumerator BreadthFirstSearch()
    {
        var dataStructures = StartCoroutine(InitializeDataStructures());

        // Do what work we can while the data structures initialize.
        var initialGameState = InitializeSearch();

        yield return dataStructures;

        var initialVertex = ReadOrigin(initialGameState);

        // Simulator things.
        _gridSimulator = new GridSimulator(initialVertex, _goal);
        _gridSimulator.GenerateGameboard();

        _verticesToSearch = new Queue<Vertex>();
        _verticesToSearch.Enqueue(initialVertex);

        while (_verticesToSearch.Count > 0)
        {
            var parent = _verticesToSearch.Dequeue();
            _visitedVertices.Add(parent);

            TryMovesWithSimulator(parent);

            foreach (var child in _adjacencies[parent.Index])
            {
                // Return if a solution was found.
                if (CheckForSolution(child))
                {
                    FoundSolution = true;
                    Solution = child;
                    yield break;
                }

                // Queue the new gamestate for search if it hasn't been encountered before.
                if (!_visitedVertices.Contains(child))
                {
                    _verticesToSearch.Enqueue(child);
                }
            }

            if (CheckMaxLoops()) { break; }
            
            // Yield after doing a chunk of work.
            if ((_vertexIndex % 150) == 0) { yield return null; }
        }
        
        _gridManager.SetGameboard(initialGameState);
        var noSolution = $"No solution found after searching {_vertexIndex} gamestates.";
        Debug.Log(noSolution);
    }

    private IEnumerator InitializeDataStructures()
    {
        Coroutine[] init = new Coroutine[3];
        init[0] = StartCoroutine(InitializeAdjacencyList());
        init[1] = StartCoroutine(InitializeVertexStack());
        init[2] = StartCoroutine(InitializeVisitedHashset());

        foreach (Coroutine dataStructure in init) {
            yield return dataStructure;
        }

        yield return StartCoroutine(PopulateAdjacencyListsAndGenerateVertices());
    }

    private IEnumerator InitializeAdjacencyList()
    {
        yield return null;
        _adjacencies = new List<List<Vertex>>(_maxVertices);
    }

    private IEnumerator InitializeVertexStack()
    {
        yield return null;
        _vertices = new(_maxVertices);
    }

    private IEnumerator InitializeVisitedHashset()
    {
        yield return null;
        _visitedVertices = new HashSet<Vertex>(_maxVertices);
    }

    private IEnumerator PopulateAdjacencyListsAndGenerateVertices()
    {
        yield return null;
        for (int i = 0; i < _maxVertices; i++) {
            List<Vertex> list = new(4);
            _adjacencies.Add(list);

            Vertex v = new();
            _vertices.Push(v);
        }
    }

    private Vertex.GameState InitializeSearch()
    {
        // The player controller has the methods that process movement.
        //_playerController = _gridManager.Player.GetComponent<PlayerController>();

        FoundSolution = false;
        _vertexIndex = 0;
        _goal = _gridManager.GetClosestCell(_gridManager.Goal.position);

        Vector3Int playerInitial = _gridManager.GetClosestCell(_gridManager.Player.position);

        List<Vector3Int> boxesInitial = new();
        foreach (var box in _gridManager.Boxes)
        {
            boxesInitial.Add(_gridManager.GetClosestCell(box.position));
        }
        
        Vertex.GameState state = new(playerInitial, boxesInitial);
        return state;
    }

    private Vertex ReadOrigin(Vertex.GameState state)
    {
        Vertex current = _vertices.Pop();
        current.LateConstructor(state);
        Origin = current;

        return current;
    }

    private bool CheckForSolution(Vertex vertex)
    {
        Vector3Int player = vertex.State.PlayerLocation;
        bool isSolved = player == _goal;

        // Log the solution.
        if (isSolved)
        {
            _moveCount = vertex.Moves.Length;
            MoveCommand[] moveArray = new MoveCommand[_moveCount];
            vertex.Moves.CopyTo(moveArray, 0);

            foreach (MoveCommand move in moveArray)
            {
                Debug.Log(move.To);
            }
            var solutionFound = $"Found a {vertex.Moves.Length}-move solution after evaluating {vertex.Index} gamestates.";
            Debug.Log(solutionFound);
        }
        return isSolved;
    }

    private void TryMovesWithSimulator(Vertex parent)
    {
        foreach (var direction in _moves)
        {
            _gridSimulator.SetGameBoard(parent.State);
            MoveCommand move = _gridSimulator.MoveProcessing(direction);

            if (move == null) { continue; }

            var activeCell = _gridSimulator.GameBoard[move.Origin.X, move.Origin.Y];
            var goalIsBlocked = activeCell == GridSimulator.Cell.Box && move.Target == _gridSimulator.Goal;
            if (goalIsBlocked) { continue; }

            _gridSimulator.MoveUnit(move);

            // Update the adjacency list with the new gamestate.
            var child = _vertices.Pop();
            _vertexIndex++;
            child.SimulatorConstructor(_gridSimulator, _vertexIndex, parent, move);
            _moveCount = child.Moves.Length;

            _adjacencies[parent.Index].Add(child);
        }
    }

    //private void TryMovesAndPopulateAdjacency(Vertex parent)
    //{
    //    foreach (var direction in _moves)
    //    {
    //        _gridManager.SetGameboard(parent.State);
    //        MoveCommand move = _playerController.MoveProcessing(direction);

    //        // Disregard this move if the command was invalid.
    //        if (move == null) { continue; }

    //        Tile fromTile = _gridManager.GetTileAtPosition(move.From);
    //        Transform activeUnit = fromTile.transform.GetChild(0);

    //        // Pushing a box into the goal makes the puzzle unsolvable, so disregard this move.
    //        bool goalIsBlocked = activeUnit.CompareTag("Box") && (move.To == _goal);
    //        if (goalIsBlocked) { continue; }

    //        _gridManager.MoveUnit(activeUnit, move.To);

    //        // Update the adjacency list with the new gamestate.
    //        Vertex childVertex = _vertices.Pop();
    //        _vertexIndex++;
    //        childVertex.LateConstructor(_vertexIndex, parent, move);
    //        _moveCount = childVertex.Moves.Length;

    //        _adjacencies[parent.Index].Add(childVertex);
    //    }
    //}

    private bool CheckMaxLoops()
    {
        if (_vertexIndex > _maxVertices - 5)
        {
            var message = $"No solution exists less than {_moveCount} moves.";
            Debug.Log(message);
            return true;
        }
        else return false;
    }
}
