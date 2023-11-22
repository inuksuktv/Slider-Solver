using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private InputAction _upAction, _downAction, _leftAction, _rightAction, _backAction;

    private Vector3Int _currentCell, _targetCell;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _upAction = _playerInput.actions["Up"];
        _downAction = _playerInput.actions["Down"];
        _leftAction = _playerInput.actions["Left"];
        _rightAction = _playerInput.actions["Right"];
        _backAction = _playerInput.actions["Back"];
    }

    private void OnEnable()
    {
        _upAction.performed += Up;
        _downAction.performed += Down;
        _leftAction.performed += Left;
        _rightAction.performed += Right;
        _backAction.performed += Back;

        _upAction.Enable();
        _downAction.Enable();
        _leftAction.Enable();
        _rightAction.Enable();
        _backAction.Enable();
    }

    private void OnDisable()
    {
        _upAction.performed -= Up;
        _downAction.performed -= Down;
        _leftAction.performed -= Left;
        _rightAction.performed -= Right;
        _backAction.performed += Back;

        _upAction.Disable();
        _downAction.Disable();
        _leftAction.Disable();
        _rightAction.Disable();
        _backAction.Disable();
    }

    private void Up(InputAction.CallbackContext context)
    {
        var command = MoveProcessing(Vector3Int.forward);
        if (command != null) {
            CommandManager.Instance.AddCommand(command);
        }
    }

    private void Down(InputAction.CallbackContext context)
    {
        var command = MoveProcessing(Vector3Int.back);
        if (command != null) {
            CommandManager.Instance.AddCommand(command);
        }
    }

    private void Left(InputAction.CallbackContext context)
    {
        var command = MoveProcessing(Vector3Int.left);
        if (command != null) {
            CommandManager.Instance.AddCommand(command);
        }
    }

    private void Right(InputAction.CallbackContext context)
    {
        var command = MoveProcessing(Vector3Int.right);
        if (command != null) {
            CommandManager.Instance.AddCommand(command);
        }

    }

    private void Back(InputAction.CallbackContext context)
    {
        CommandManager.Instance.Undo();
    }

    
    public void FindDestination(Vector3Int direction)
    {
        // Search in the direction of the move until a tile that blocks movement is found.
        int maxMove = Mathf.Max(GridManager.Instance.BoardWidth + 1, GridManager.Instance.BoardHeight + 1);
        for (int i = 1; i < maxMove + 1; i++) {
            Tile nextTile = GridManager.Instance.GetTileAtPosition(_currentCell + direction * i);
            if (nextTile.BlocksMove) {
                _targetCell = GridManager.Instance.GetClosestCell(nextTile.transform.position - direction);
                break;
            }
        }
    }

    public void GetActiveUnit(Vector3Int direction)
    {
        // The player is the active unit by default. If the player pushed a box, the box is the active unit instead.
        _currentCell = GridManager.Instance.GetClosestCell(transform.position);
        _targetCell = GridManager.Instance.GetClosestCell(transform.position + direction);

        // Check for a box at the target tile.
        Transform tile = GridManager.Instance.GetTileAtPosition(_targetCell).transform;
        if (tile.childCount > 0 && tile.GetChild(0).CompareTag("Box")) {
            _currentCell = _targetCell;
            _targetCell = GridManager.Instance.GetClosestCell(tile.position + direction);
        }
    }

    public MoveCommand MoveProcessing(Vector3Int direction)
    {
        GetActiveUnit(direction);

        // Return without effect if the move is illegal.
        Tile testTile = GridManager.Instance.GetTileAtPosition(_targetCell);
        if (testTile == null || testTile.BlocksMove) {
            return null;
        }

        FindDestination(direction);

        MoveCommand command = new(_currentCell, _targetCell);
        return command;
    }
}
