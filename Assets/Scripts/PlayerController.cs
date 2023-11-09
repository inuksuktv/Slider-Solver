using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction upAction;
    private InputAction downAction;
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction backAction;

    private Vector3Int goalTilePosition;
    public Vector3Int currentCell, targetCell;
    public Transform unit;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        upAction = playerInput.actions["Up"];
        downAction = playerInput.actions["Down"];
        leftAction = playerInput.actions["Left"];
        rightAction = playerInput.actions["Right"];
        backAction = playerInput.actions["Back"];
    }

    private void OnEnable()
    {
        upAction.performed += Up;
        downAction.performed += Down;
        leftAction.performed += Left;
        rightAction.performed += Right;
        backAction.performed += Back;

        upAction.Enable();
        downAction.Enable();
        leftAction.Enable();
        rightAction.Enable();
        backAction.Enable();
    }

    private void OnDisable()
    {
        upAction.performed -= Up;
        downAction.performed -= Down;
        leftAction.performed -= Left;
        rightAction.performed -= Right;
        backAction.performed += Back;

        upAction.Disable();
        downAction.Disable();
        leftAction.Disable();
        rightAction.Disable();
        backAction.Disable();
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

    private void Start()
    {
        goalTilePosition = GridManager.Instance.GetClosestCell(FindFirstObjectByType<GoalTile>().transform.position);
    }

    private void Update()
    {
        if (GridManager.Instance.GetClosestCell(transform.position) == goalTilePosition) {
            Debug.Log("You win the game!");
        }
    }
    public void FindDestination(Vector3Int direction)
    {
        // Search in the direction of the move until a tile that blocks movement is found.
        int maxMove = Mathf.Max(GridManager.Instance.boardWidth + 1, GridManager.Instance.boardHeight + 1);
        for (int i = 1; i < maxMove + 1; i++) {
            Tile nextTile = GridManager.Instance.GetTileAtPosition(currentCell + direction * i);
            if (nextTile.BlocksMove) {
                targetCell = GridManager.Instance.GetClosestCell(nextTile.transform.position - direction);
                break;
            }
        }
    }

    public void GetActiveUnit(Vector3Int direction)
    {
        currentCell = GridManager.Instance.GetClosestCell(transform.position);
        targetCell = GridManager.Instance.GetClosestCell(transform.position + direction);
        unit = transform;

        // If the player moved into a box, make the box the active unit. Otherwise the player is the active unit.
        foreach (Transform box in GridManager.Instance.boxes) {
            if (targetCell == GridManager.Instance.GetClosestCell(box.position)) {
                currentCell = targetCell;
                targetCell = GridManager.Instance.GetClosestCell(box.position) + direction;
                unit = box;
            }
        }
    }

    public MoveCommand MoveProcessing(Vector3Int direction)
    {
        GetActiveUnit(direction);

        // Return without effect if the move is illegal.
        Tile testTile = GridManager.Instance.GetTileAtPosition(targetCell);
        if (testTile == null || testTile.BlocksMove) {
            return null;
        }

        FindDestination(direction);

        MoveCommand command = new(currentCell, targetCell, unit);
        return command;
    }
}
