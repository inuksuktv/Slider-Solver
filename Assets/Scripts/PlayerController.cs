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
    private Vector2 move;

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
        // In the simplest case we don't want input to do anything while a move is being animated, which is handled by a single bool. If we
        // add another bool to manage another state though, I should just turn it into a state machine.
        Vector3Int cellPosition = GetClosestCell(transform.position);
        Vector3Int targetCell = GetClosestCell(transform.position + Vector3Int.forward);
        MoveCommand cmd = new MoveCommand(cellPosition, targetCell);
        CommandManager.Instance.AddCommand(cmd);
    }

    private void Down(InputAction.CallbackContext context)
    {
        Vector3Int cellPosition = GetClosestCell(transform.position);
        Vector3Int targetCell = GetClosestCell(transform.position + Vector3Int.back);
        MoveCommand cmd = new MoveCommand(cellPosition, targetCell);
        CommandManager.Instance.AddCommand(cmd);
    }

    private void Left(InputAction.CallbackContext context)
    {
        Vector3Int cellPosition = GetClosestCell(transform.position);
        Vector3Int targetCell = GetClosestCell(transform.position + Vector3Int.left);
        MoveCommand cmd = new MoveCommand(cellPosition, targetCell);
        CommandManager.Instance.AddCommand(cmd);
    }

    private void Right(InputAction.CallbackContext context)
    {
        Vector3Int cellPosition = GetClosestCell(transform.position);
        Vector3Int targetCell = GetClosestCell(transform.position + Vector3Int.right);
        MoveCommand cmd = new MoveCommand(cellPosition, targetCell);
        CommandManager.Instance.AddCommand(cmd);
    }

    private Vector3Int GetClosestCell(Vector3 position)
    {
        Vector3Int closestCell = GridManager.Instance.Grid.WorldToCell(position);
        return closestCell;
    }
}
