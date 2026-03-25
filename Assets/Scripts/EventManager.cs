using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public event Action<Vector2> onTeleportPlayer;
    public void TeleportPlayer(Vector2 pos)
    {
        if (onTeleportPlayer != null) { onTeleportPlayer(pos); }
    }
}
