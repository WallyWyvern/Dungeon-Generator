using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public EdgeDirection direction;
    [SerializeField] private BoxCollider doorTrigger;
    [SerializeField] private Vector2 teleportUp;
    [SerializeField] private Vector2 teleportDown;
    [SerializeField] private Vector2 teleportLeft;
    [SerializeField] private Vector2 teleportRight;


    public void SetDoorSprite(Sprite door)
    { 
        spriteRenderer.sprite = door;
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (direction)
        {
            case EdgeDirection.Up:
                EventManager.Instance.TeleportPlayer(teleportUp);
                break;
            case EdgeDirection.Down:
                EventManager.Instance.TeleportPlayer(teleportDown);
                break;
            case EdgeDirection.Left:
                EventManager.Instance.TeleportPlayer(teleportLeft);
                break;
            case EdgeDirection.Right:
                EventManager.Instance.TeleportPlayer(teleportRight);
                break;
            default:
                break;

        }
    }


}
