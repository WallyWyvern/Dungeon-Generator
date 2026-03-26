using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCam : MonoBehaviour
{
    [SerializeField] private InputActionReference newDungeon;
    [SerializeField] private Vector3 startPosition;
    private void Start()
    {
        EventManager.Instance.onTeleportPlayer += TeleportCam;
        newDungeon.action.started += ResetCam;
    }

    private void ResetCam(InputAction.CallbackContext context)
    {
        transform.position = startPosition;
    }

    private void TeleportCam(EdgeDirection direction)
    {
        Vector2 temp1;
        switch (direction)
        {
            case EdgeDirection.Up:
                temp1 = new Vector2 (0, RoomManager.instance.yOffset * 0.5f);
                break;
            case EdgeDirection.Down:
                temp1 = new Vector2(0, -RoomManager.instance.yOffset * 0.5f);
                break;
            case EdgeDirection.Left:
                temp1 = new Vector2(-RoomManager.instance.xOffset * 0.5f, 0);
                break;
            case EdgeDirection.Right:
                temp1 = new Vector2(RoomManager.instance.xOffset*0.5f, 0);
                break;
            default:
                Debug.Log("No edge direction was found");
                temp1 = Vector2.zero;
                break;
        }

        Vector3 temp2 = new Vector3(transform.position.x + temp1.x, transform.position.y + temp1.y, transform.position.z);
        transform.position = temp2;
    }
}
