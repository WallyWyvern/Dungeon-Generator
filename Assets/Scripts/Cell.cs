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
    public int value;
    public List<int> cellList = new List<int>();

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
