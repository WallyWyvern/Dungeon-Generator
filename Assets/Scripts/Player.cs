using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference bombAction;
    [SerializeField] private CharacterController player;
    [SerializeField] private float playerSpeed = 0.05f;
    private Vector2 moveDirection;

    private void OnEnable()
    {
        bombAction.action.started += dropBomb;
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        bombAction.action.started -= dropBomb;
        moveAction.action.Disable();
    }

    void Start()
    {

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
