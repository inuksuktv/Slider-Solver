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
    private PlayerController _playerController;

    private Vector3Int _goal;
    private readonly Vector3Int[] _moves = { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };

    private void Start()
    {
        // Use a local reference to reduce verbosity. GridManager is a singleton.
        _gridManager = GridManager.Instance;
    }

    public IEnumerator BreadthFirstSearch()
    {
        var dataStructures = StartCoroutine(InitializeDataStructures());

        var (playerPosition, boxLocations) = InitializeSearch();

        // These need to finish before we can populate the starting vertex's data.
        yield return dataStructures;

        Vertex currentGamestate = ReadInitialGamestate(playerPosition, boxLocations);

        _verticesToSearch = new Queue<Vertex>();
        _verticesToSearch.Enqueue(currentGamestate);

        // Begin the search.
        while (_verticesToSearch.Count > 0)
        {
            Vertex parent = _verticesToSearch.Dequeue();
            _visitedVertices.Add(parent);

            // Populate the adjacency list for the current vertex by trying to move in each direction.
            TryMoves(parent);

            foreach (Vertex child in _adjacencies[parent.Index])
            {
                // Return if a solution was found.
                if (CheckForSolution(child))
                {
                    FoundSolution = true;
                    Solution = child;
                    _gridManager.SetGameboard(playerPosition, boxLocations);
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
            if ((_vertexIndex % 30) == 0) { yield return null; }
        }
        
        _gridManager.SetGameboard(playerPosition, boxLocations);
        var noSolution = $"No solution found after searching {_vertexIndex} gamestates.";
        Debug.Log(noSolution);
    }

    private (Vector3Int playerPosition, List<Vector3Int> boxLocations) InitializeSearch()
    {
        // The player controller has the methods that process movement.
        _playerController = _gridManager.Player.GetComponent<PlayerController>();

        FoundSolution = false;
        _vertexIndex = 0;
        _goal = _gridManager.GetClosestCell(_gridManager.Goal.position);

        Vector3Int playerInitial = _gridManager.GetClosestCell(_gridManager.Player.position);

        List<Vector3Int> boxesInitial = new();
        foreach (var box in _gridManager.Boxes)
        {
            boxesInitial.Add(_gridManager.GetClosestCell(box.position));
        }

        return (playerInitial, boxesInitial);
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

    private Vertex ReadInitialGamestate(Vector3Int playerPosition, List<Vector3Int> boxLocations)
    {
        Vertex currentGamestate = _vertices.Pop();
        currentGamestate.LateConstructor(playerPosition, boxLocations);
        Origin = currentGamestate;

        return currentGamestate;
    }

    private bool CheckForSolution(Vertex vertex)
    {
        MoveCommand command = vertex.Moves.Last();
        bool isSolved = command.Unit.CompareTag("Player") && (command.To == _goal);

        // Log the solution.
        if (isSolved)
        {
            _moveCount = vertex.Moves.Count;
            MoveCommand[] moveArray = new MoveCommand[_moveCount];
            vertex.Moves.CopyTo(moveArray, 0);

            foreach (MoveCommand move in moveArray)
            {
                Debug.Log(move.To);
            }
            var solutionFound = $"Found a {vertex.Moves.Count}-move solution after evaluating {vertex.Index} gamestates.";
            Debug.Log(solutionFound);
        }
        return isSolved;
    }

    private void TryMoves(Vertex parent)
    {
        foreach (var direction in _moves)
        {
            _gridManager.SetGameboard(parent.PlayerLocation, parent.Boxes);
            MoveCommand command = _playerController.MoveProcessing(direction);

            // Disregard this move if the command was invalid.
            if (command == null) { continue; }

            Tile fromTile = _gridManager.GetTileAtPosition(command.From);
            Transform activeUnit = fromTile.transform.GetChild(0);

            // Pushing a box into the goal makes the puzzle unsolvable, so disregard this move.
            bool goalIsBlocked = activeUnit.CompareTag("Box") && (command.To == _goal);
            if (goalIsBlocked) { continue; }

            _gridManager.MoveUnit(activeUnit, command.To);

            // Update the adjacency list with the new gamestate.
            Vertex childVertex = _vertices.Pop();
            childVertex.LateConstructor(++_vertexIndex, parent, command);
            _moveCount = childVertex.Moves.Count;

            _adjacencies[parent.Index].Add(childVertex);
        }
    }

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
