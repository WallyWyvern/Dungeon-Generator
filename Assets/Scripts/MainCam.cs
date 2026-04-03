using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCam : MonoBehaviour
{
    [SerializeField] private InputActionReference newDungeon;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private float cellSize = 0.5f;
    private void Start()
    {
        EventManager.Instance.onTeleportPlayer += TeleportCam;
        newDungeon.action.started += ResetCam;
    }

    private void ResetCam( InputAction.CallbackContext context )
    {
        transform.position = startPosition;
    }

    private void TeleportCam( EdgeDirection direction )
    {
        Vector2 temp1;
        switch ( direction )
        {
            case EdgeDirection.Up:
                temp1 = new Vector2 ( 0, RoomManager.instance.yRoomSize * cellSize );
                break;
            case EdgeDirection.Down:
                temp1 = new Vector2( 0, -RoomManager.instance.yRoomSize * cellSize);
                break;
            case EdgeDirection.Left:
                temp1 = new Vector2( -RoomManager.instance.xRoomSize * cellSize, 0 );
                break;
            case EdgeDirection.Right:
                temp1 = new Vector2( RoomManager.instance.xRoomSize * cellSize, 0 );
                break;
            default:
                Debug.Log( "No edge direction was found" );
                temp1 = Vector2.zero;
                break;
        }

        Vector3 temp2 = new Vector3( transform.position.x + temp1.x, transform.position.y + temp1.y, transform.position.z );
        transform.position = temp2;
    }
}
