using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// This class represents a directed graph using an adjacency list.
public class Graph : MonoBehaviour
{
    [SerializeField] private int _maxVertices;
    public Vertex Origin { get; private set; }
    public Vertex Solution { get; private set; }

    private List<List<Vertex>> _adjacency;
    private HashSet<Vertex> _visited;
    private Stack<Vertex> _vertices;
    private int _vertexIndex;
    private int _moveCount;
    private PlayerController _playerController;

    private Vector3Int _currentPlayer;
    private Vector3Int _goal;
    private List<Vector3Int> _boxLocations;
    private Vector3Int _direction;

    public IEnumerator BreadthFirstSearch(Vector3Int position, List<Transform> boxList)
    {
        _playerController = GridManager.Instance.Player.GetComponent<PlayerController>();

        // Store the initial game state.
        Vector3Int playerInitial = position;
        _goal = GridManager.Instance.GetClosestCell(GridManager.Instance.Goal.position);
        List<Vector3Int> boxesInitial = new();
        foreach (Transform box in boxList) {
            boxesInitial.Add(GridManager.Instance.GetClosestCell(box.position));
        }

        // Initialize data structures for the search.
        yield return StartCoroutine(InitializeDataStructuresWithCoroutines());

        // Write the starting vertex's data, visit it, and queue it for search.
        Vertex start = _vertices.Pop();
        _vertexIndex = 0;
        start.LateConstructor(playerInitial, boxesInitial);
        Origin = start;
        _visited.Add(start);

        Queue<Vertex> search = new();
        search.Enqueue(start);

        // Begin the search.
        while (search.Count > 0) {
            // Dequeue the next vertex and read its game state.
            Vertex parent = search.Dequeue();
            _currentPlayer = parent.PlayerLocation;
            _boxLocations = parent.Boxes;

            // Populate the adjacency list for the current vertex by trying to move in each direction.
            TryMoves(parent);

            foreach (Vertex child in _adjacency[parent.Index]) {
                // Return if the player arrived at the goal.
                if (CheckForSolution(child)) {
                    GridManager.Instance.SetGameboard(playerInitial, boxesInitial);
                    Solution = child;
                    yield break;
                }
                // Check if the vertex is in the hashset to see if the game state has been visited previously.
                if (!_visited.Contains(child)) {
                    _visited.Add(child);
                    search.Enqueue(child);
                }
            }
            // If we hit the set maximum search, break out of the search.
            if (_vertexIndex > _maxVertices - 5) {
                Debug.Log("No solution exists less than " + _moveCount + " moves.");
                break;
            }
            // Yield to the main thread after doing a chunk of work.
            if (_vertexIndex % 100 == 0) { yield return null; }
        }
        GridManager.Instance.SetGameboard(playerInitial, boxesInitial);
        Debug.Log("Returned with no solution after searching " + _vertexIndex + " game states.");
    }

    private IEnumerator InitializeDataStructuresWithCoroutines()
    {
        Coroutine[] init = new Coroutine[3];
        init[0] = StartCoroutine(AdjacencyList());
        init[1] = StartCoroutine(Vertices());
        init[2] = StartCoroutine(Visited());

        // Wait for those data structures to finish initializing.
        foreach (Coroutine dataStructure in init) {
            yield return dataStructure;
        }
        // Then loop through the adjacency list and stack of vertices to initialize them.
        yield return StartCoroutine(StackAndLists());
    }

    private IEnumerator AdjacencyList()
    {

        yield return null;
        _adjacency = new List<List<Vertex>>(_maxVertices);
    }

    private IEnumerator Vertices()
    {
        yield return null;
        _vertices = new(_maxVertices);
    }

    private IEnumerator Visited()
    {
        yield return null;
        _visited = new HashSet<Vertex>(_maxVertices);
    }

    private IEnumerator StackAndLists()
    {
        yield return null;
        for (int i = 0; i < _maxVertices; i++) {
            List<Vertex> list = new(4);
            _adjacency.Add(list);

            Vertex v = new();
            _vertices.Push(v);
        }
    }


    private bool CheckForSolution(Vertex vertex)
    {
        MoveCommand command = vertex.Moves.Last();
        bool isSolved = command.Unit.CompareTag("Player") && command.To == _goal;
        if (isSolved) {
            // Log the solution.
            _moveCount = vertex.Moves.Count;
            MoveCommand[] moveArray = new MoveCommand[_moveCount];
            vertex.Moves.CopyTo(moveArray, 0);
            foreach (MoveCommand move in moveArray) {
                Debug.Log(move.To);
            }
            Debug.Log("Found a " + vertex.Moves.Count + "-move solution after evaluating " + vertex.Index + " game states.");
        }
        return isSolved;
    }


    private void PlaceGamePieces(Vector3Int playerFromVertex, List<Vector3Int> boxLocations)
    {
        GridManager.Instance.Player.position = playerFromVertex;
        for (int i = 0; i < boxLocations.Count; i++) {
            GridManager.Instance.Boxes[i].position = boxLocations[i];
        }
        GridManager.Instance.UpdateTiles();
    }

    private void TryMoves(Vertex parent)
    {
        for (int moveIndex = 0; moveIndex < 4; moveIndex++) {
            GridManager.Instance.SetGameboard(_currentPlayer, _boxLocations);

            switch (moveIndex) {
                case 0:
                    _direction = Vector3Int.forward;
                    break;
                case 1:
                    _direction = Vector3Int.back;
                    break;
                case 2:
                    _direction = Vector3Int.left;
                    break;
                case 3:
                    _direction = Vector3Int.right;
                    break;
            }

            MoveCommand command = _playerController.MoveProcessing(_direction);

            // Illegal moves return null. Only legal moves get processed.
            if (command != null) {
                // Pushing a box onto the goal tile means there's no solution, so don't consider that move.
                Transform unit = GridManager.Instance.GetTileAtPosition(command.From).transform.GetChild(0);
                bool isUnsolvable = unit.CompareTag("Box") && command.To == _goal;
                if (isUnsolvable) { continue; }

                // Record the new game vertex and update the adjacency list.
                GridManager.Instance.MoveUnit(unit, command.To);
                GridManager.Instance.UpdateTiles();

                Vertex childVertex = _vertices.Pop();
                _vertexIndex++;
                childVertex.LateConstructor(_vertexIndex, parent, command);
                _moveCount = childVertex.Moves.Count;

                _adjacency[parent.Index].Add(childVertex);
            }
        }
    }
}
