using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputAction _upAction, _downAction, _leftAction, _rightAction, _backAction, _pauseAction;

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

    private void Update()
    {
        if (CommandManager.Instance != null && GridManager.Instance != null) {
            if (CommandManager.Instance.UnitIsMoving || GridManager.Instance.SearchIsRunning) {
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
                CommandManager.Instance.AddCommand(command);
            }
        }
    }

    private void Down(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.back);
            if (command != null) {
                CommandManager.Instance.AddCommand(command);
            }
        }
    }

    private void Left(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.left);
            if (command != null) {
                CommandManager.Instance.AddCommand(command);
            }
        }
    }

    private void Right(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            var command = MoveProcessing(Vector3Int.right);
            if (command != null) {
                CommandManager.Instance.AddCommand(command);
            }
        }
    }

    private void Back(InputAction.CallbackContext context)
    {
        if (!_inputIsBlocked) {
            CommandManager.Instance.Undo();
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
        Tile testTile = GridManager.Instance.GetTileAtPosition(_targetCell);
        if (testTile == null || testTile.BlocksMove)
        {
            return null;
        }

        FindDestination(direction);

        MoveCommand command = new(_originCell, _targetCell);
        return command;
    }

    private void SetOriginAndTargetCells(Vector3Int direction)
    {
        // The player is the active unit by default. If the player pushed a box, the box is the active unit instead.
        _originCell = GridManager.Instance.GetClosestCell(transform.position);
        _targetCell = GridManager.Instance.GetClosestCell(transform.position + direction);

        // If the player is pushing a box, adjust so the box is the active unit.
        Transform targetTile = GridManager.Instance.GetTileAtPosition(_targetCell).transform;
        if ((targetTile.childCount > 0) && targetTile.GetChild(0).CompareTag("Box"))
        {
            _originCell = _targetCell;
            _targetCell = GridManager.Instance.GetClosestCell(targetTile.position + direction);
        }
    }

    private void FindDestination(Vector3Int direction)
    {
        // Search in the direction of the move until a tile that blocks movement is found.
        int maxMove = Mathf.Max(GridManager.Instance.BoardWidth + 1, GridManager.Instance.BoardHeight + 1);
        for (int i = 1; i <= maxMove; i++)
        {
            Tile nextTile = GridManager.Instance.GetTileAtPosition(_originCell + direction * i);

            if (nextTile.BlocksMove)
            {
                _targetCell = GridManager.Instance.GetClosestCell(nextTile.transform.position - direction);
                break;
            }
        }
    }
}
