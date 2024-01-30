using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputAction _upAction, _downAction, _leftAction, _rightAction, _backAction, _pauseAction;

    private GridManager _gridManager;
    private CommandManager _commandManager;
    private Vector3Int _originCell, _targetCell;
    private bool _inputIsBlocked = false;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _upAction = _playerInput.actions["Up"];
        _downAction = _playerInput.actions["Down"];
        _leftAction = _playerInput.actions["Left"];
        _rightAction = _playerInput.actions["Right"];
        _backAction = _playerInput.actions["Back"];
        _pauseAction = _playerInput.actions["Escape"];
    }

    private void OnEnable()
    {
        _upAction.performed += Up;
        _downAction.performed += Down;
        _leftAction.performed += Left;
        _rightAction.performed += Right;
        _backAction.performed += Back;
        _pauseAction.performed += Pause;

        _upAction.Enable();
        _downAction.Enable();
        _leftAction.Enable();
        _rightAction.Enable();
        _backAction.Enable();
        _pauseAction.Enable();
    }

    private void OnDisable()
    {
        _upAction.performed -= Up;
        _downAction.performed -= Down;
        _leftAction.performed -= Left;
        _rightAction.performed -= Right;
        _backAction.performed -= Back;
        _pauseAction.performed -= Pause;

        _upAction.Disable();
        _downAction.Disable();
        _leftAction.Disable();
        _rightAction.Disable();
        _backAction.Disable();
        _pauseAction.Disable();
    }

    private void Start()
    {
        // Local cache to reduce verbosity. Both objects are singletons.
        _gridManager = GridManager.Instance;
        _commandManager = CommandManager.Instance;
    }

    private void Update()
    {
        if (_commandManager != null && _gridManager != null) {
            if (_commandManager.UnitIsMoving || _gridManager.SearchIsRunning) {
                _inputIsBlocked = true;
            }
            else {
                _inputIsBlocked = false;
            }
        }
    }

    private void Up(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.forward);
            if (command != null) {
                _commandManager.AddCommand(command);
            }
        }
    }

    private void Down(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.back);
            if (command != null) {
                _commandManager.AddCommand(command);
            }
        }
    }

    private void Left(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.left);
            if (command != null) {
                _commandManager.AddCommand(command);
            }
        }
    }

    private void Right(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.right);
            if (command != null) {
                _commandManager.AddCommand(command);
            }
        }
    }

    private void Back(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            _commandManager.Undo();
        }
    }

    private void Pause(InputAction.CallbackContext context)
    {
        var pauseMenu = GameObject.Find("Canvas").transform.GetComponentInChildren<PauseMenu>(true);
        if (pauseMenu != null) {
            pauseMenu.TogglePauseMenu();
        }
    }

    public MoveCommand MoveProcessing(Vector3Int direction)
    {
        SetOriginAndTargetCells(direction);

        // Return null if the move is illegal.
        Tile testTile = _gridManager.GetTileAtPosition(_targetCell);
        if (testTile == null || testTile.BlocksMove)
        {
            return null;
        }

        FindDestination(direction);

        MoveCommand move = new(_originCell, _targetCell);

        return move;
    }

    private void SetOriginAndTargetCells(Vector3Int direction)
    {
        // The player is the active unit by default. If the player pushed a box, the box is the active unit instead.
        _originCell = _gridManager.GetClosestCell(transform.position);
        _targetCell = _gridManager.GetClosestCell(transform.position + direction);

        // If the player is pushing a box, adjust so the box is the active unit.
        Transform targetTile = _gridManager.GetTileAtPosition(_targetCell).transform;
        if ((targetTile.childCount > 0) && targetTile.GetChild(0).CompareTag("Box"))
        {
            _originCell = _targetCell;
            _targetCell = _gridManager.GetClosestCell(targetTile.position + direction);
        }
    }

    private void FindDestination(Vector3Int direction)
    {
        // Search in the direction of the move until a tile that blocks movement is found.
        int maxMove = Mathf.Max(_gridManager.BoardWidth + 1, _gridManager.BoardHeight + 1);
        for (var i = 1; i <= maxMove; i++)
        {
            Tile nextTile = _gridManager.GetTileAtPosition(_originCell + direction * i);
            if (nextTile.BlocksMove)
            {
                _targetCell = _gridManager.GetClosestCell(nextTile.transform.position - direction);
                break;
            }
        }
    }
}
