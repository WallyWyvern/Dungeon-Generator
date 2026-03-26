using UnityEngine;

public class Door : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public EdgeDirection direction;
    [SerializeField] private BoxCollider doorTrigger;



    public void SetDoorSprite(Sprite door)
    { 
        spriteRenderer.sprite = door;
    }

    private void OnTriggerEnter(Collider other)
    {
        EventManager.Instance.TeleportPlayer(direction);
    }
}
