using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public event Action<EdgeDirection> onTeleportPlayer;
    public void TeleportPlayer( EdgeDirection dir )
    {
        if ( onTeleportPlayer != null ) { onTeleportPlayer( dir ); }
    }
}
