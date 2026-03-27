using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum RoomType
{
    Regular,
    Item,
    Shop,
    Boss,
    Secret
}

public class Cell : MonoBehaviour
{
    public int index;
    public Vector2Int key;
    public int value;
    public List<int> cellListLegacy = new List<int>();
    public List<Vector2Int> cellList = new List<Vector2Int>();

    public SpriteRenderer spriteRenderer;

    public RoomType roomType;

    public void SetSpecialRoomSprite(Sprite icon)
    { 
        spriteRenderer.sprite = icon;
    }

    public void SetRoomType(RoomType newRoomType)
    { 
        roomType = newRoomType;
    }


}
