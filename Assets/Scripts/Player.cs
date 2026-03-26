using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Vector2 teleportUp;
    [SerializeField] private Vector2 teleportDown;
    [SerializeField] private Vector2 teleportLeft;
    [SerializeField] private Vector2 teleportRight;

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference bombAction;
    [SerializeField] private CharacterController player;
    [SerializeField] private float playerSpeed = 0.05f;
    private Vector2 moveDirection;

    private void Start()
    {
        bombAction.action.started += dropBomb;
        moveAction.action.Enable();
        EventManager.Instance.onTeleportPlayer += TeleportPlayer;
    }

    private void TeleportPlayer(EdgeDirection direction)
    {
        Vector2 temp1;
        switch (direction)
        {
            case EdgeDirection.Up:
                temp1 = teleportUp;
                break;
            case EdgeDirection.Down:
                temp1 = teleportDown;
                break;
            case EdgeDirection.Left:
                temp1 = teleportLeft;
                break;
            case EdgeDirection.Right:
                temp1 = teleportRight;
                break;
            default:
                Debug.Log("No edge direction was found");
                temp1 = Vector2.zero;
                break;
        }

        Vector3 temp2 = new Vector3(transform.position.x + temp1.x, transform.position.y + temp1.y, transform.position.z);
        transform.position = temp2;
        Physics.SyncTransforms();
    }

    private void OnDestroy()
    {
        bombAction.action.started -= dropBomb;
        moveAction.action.Disable();
    }

    private void dropBomb(InputAction.CallbackContext context)
    {
        Debug.Log("Dropped bomb!");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveDirection = moveAction.action.ReadValue<Vector2>();
        player.Move(moveDirection.normalized * playerSpeed);
    }
}
