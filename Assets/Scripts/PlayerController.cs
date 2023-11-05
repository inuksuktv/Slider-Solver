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

    private Tile goalTile;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        upAction = playerInput.actions["Up"];
        downAction = playerInput.actions["Down"];
        leftAction = playerInput.actions["Left"];
        rightAction = playerInput.actions["Right"];
    }

    private void OnEnable()
    {
        upAction.performed += Up;
        downAction.performed += Down;
        leftAction.performed += Left;
        rightAction.performed += Right;

        upAction.Enable();
        downAction.Enable();
        leftAction.Enable();
        rightAction.Enable();
    }

    private void OnDisable()
    {
        upAction.performed -= Up;
        downAction.performed -= Down;
        leftAction.performed -= Left;
        rightAction.performed -= Right;

        upAction.Disable();
        downAction.Disable();
        leftAction.Disable();
        rightAction.Disable();
    }

    private void Up(InputAction.CallbackContext context)
    {
        Vector3Int moveDirection = Vector3Int.forward;
        Vector3Int origin = GridManager.Instance.GetClosestCell(transform.position);
        Vector3Int targetCell = GridManager.Instance.GetClosestCell(transform.position + moveDirection);

        // If player is pushing a box, move the box instead.
        foreach (GameObject box in GridManager.Instance.boxes) {
            if (targetCell == GridManager.Instance.GetClosestCell(box.transform.position)) {
                origin = GridManager.Instance.GetClosestCell(box.transform.position);
                targetCell = GridManager.Instance.GetClosestCell(box.transform.position + moveDirection);
            }
        }

        // Check if the move is illegal.
        if (GridManager.Instance.GetTileAtPosition(targetCell).BlocksMove) { return; }

        // Search in the direction of the move until a tile that blocks movement is found.
        int maxMove = Mathf.Max(GridManager.Instance.width, GridManager.Instance.height);
        for (int i = 1; i < maxMove; i++) {
            Tile testTile = GridManager.Instance.GetTileAtPosition(origin + moveDirection * i);
            if (testTile.BlocksMove) {
                targetCell = GridManager.Instance.GetClosestCell(testTile.transform.position - moveDirection);
            }
        }

        // Send the move command.
        MoveCommand command = new(origin, targetCell, transform);
        CommandManager.Instance.AddCommand(command);
    }

    private void Down(InputAction.CallbackContext context)
    {
        Vector3Int cellPosition = GridManager.Instance.GetClosestCell(transform.position);
        Vector3Int targetCell = GridManager.Instance.GetClosestCell(transform.position + Vector3Int.back);
        MoveCommand command = new(cellPosition, targetCell, transform);
        CommandManager.Instance.AddCommand(command);
    }

    private void Left(InputAction.CallbackContext context)
    {
        Vector3Int cellPosition = GridManager.Instance.GetClosestCell(transform.position);
        Vector3Int targetCell = GridManager.Instance.GetClosestCell(transform.position + Vector3Int.left);
        MoveCommand command = new(cellPosition, targetCell, transform);
        CommandManager.Instance.AddCommand(command);
    }

    private void Right(InputAction.CallbackContext context)
    {
        Vector3Int cellPosition = GridManager.Instance.GetClosestCell(transform.position);
        Vector3Int targetCell = GridManager.Instance.GetClosestCell(transform.position + Vector3Int.right);
        MoveCommand command = new(cellPosition, targetCell, transform);
        CommandManager.Instance.AddCommand(command);
    }

    private void Start()
    {
        goalTile = FindFirstObjectByType<GoalTile>();
    }

    private void Update()
    {
        if (GridManager.Instance.GetClosestCell(transform.position) == GridManager.Instance.GetClosestCell(goalTile.transform.position)) {
            Debug.Log("You win the game!");
        }
    }
}
