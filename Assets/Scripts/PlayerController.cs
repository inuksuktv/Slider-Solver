using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;

    private InputAction moveAction;
    private Vector2 move;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void Update()
    {
        // If we're animating, early return so we don't fire TryMove.

        move = moveAction.ReadValue<Vector2>();
        if (move != Vector2.zero) { TryMove(); }
    }

    private void TryMove()
    {
        Vector3Int playerCell = GetClosestCell(transform.position);
        Vector3Int targetCell = new(playerCell.x + (int)move.x, playerCell.y, playerCell.z + (int)move.y);
        MoveCommand command = new(playerCell,targetCell);
        CommandManager.Instance.AddCommand(command);
    }

    private Vector3Int GetClosestCell(Vector3 position)
    {
        var idx = GameObject.Find("GridManager").GetComponent<GridManager>().Grid.WorldToCell(position);
        return idx;
    }
}
