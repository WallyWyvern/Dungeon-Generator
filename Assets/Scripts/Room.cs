using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EdgeDirection
{
    Up,
    Down, 
    Left, 
    Right
}

public class Room : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void SetupRoom(Cell currentCell, RoomScriptable room)
    {
        spriteRenderer.sprite = room.roomVariations[Random.Range(0, room.roomVariations.Length)];

        if(currentCell.roomType == RoomType.Secret) { return; }

        var floorPlan = MapGenerator.instance.getFloorPlan;
        var cellList = MapGenerator.instance.getSpawnedCells;

        SetupOneByOne(currentCell, floorPlan, cellList);
    }

    public void SetupOneByOne(Cell cell, int[] floorPlan, List<Cell> cellList)
    {
        var currentCell = cell.cellList[0];

        TryPlaceDoor(currentCell, new Vector2(0, 1.75f), EdgeDirection.Up, floorPlan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(0, -1.75f), EdgeDirection.Down, floorPlan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(-4.25f, 0), EdgeDirection.Left, floorPlan, cellList, cell);
        TryPlaceDoor(currentCell, new Vector2(4.25f, 0), EdgeDirection.Right, floorPlan, cellList, cell);
    }

    private void TryPlaceDoor(int fromIndex, Vector2 positionOffset, EdgeDirection direction, int[] floorPlan, List<Cell> cellList, Cell currentCell)
    {
        int neighbourIndex = fromIndex + GetOffset(direction);

        if (neighbourIndex < 0 || neighbourIndex >= floorPlan.Length) { return; }
        if (floorPlan[neighbourIndex] != 1 ) { return; }
        
        var foundCell = cellList.FirstOrDefault(x => x.cellList.Contains(neighbourIndex));

        if (foundCell.roomType == RoomType.Secret ) { return; }

        var door = Instantiate(RoomManager.instance.doorPrefab, transform);
        door.transform.position = (Vector2)transform.position + positionOffset;

        SetupDoor(door, direction, currentCell.roomType == RoomType.Regular ? foundCell.roomType : currentCell.roomType);
    }

    private void SetupDoor(Door door, EdgeDirection direction, RoomType roomType)
    {
        var doorType = GetDoorOptions(roomType);
        door.SetDoorSprite(doorType.upDoor);

        switch (direction)
        {
            case EdgeDirection.Up:
                door.direction = EdgeDirection.Up;
                break;
            case EdgeDirection.Down:
                door.direction = EdgeDirection.Down;
                door.GetComponent<BoxCollider>().transform.Rotate(0, 0, 180);
                break;
            case EdgeDirection.Left:
                door.direction = EdgeDirection.Left;
                door.GetComponent<BoxCollider>().transform.Rotate(0, 0, 90);
                break;
            case EdgeDirection.Right:
                door.direction = EdgeDirection.Right;
                door.GetComponent<BoxCollider>().transform.Rotate(0, 0, -90);
                break;
            default:
                break;

        }
    }

    private DoorScriptable GetDoorOptions(RoomType roomType)
    { 
        return RoomManager.instance.doors.FirstOrDefault(x=>x.roomType == roomType); 
    }

    private int GetOffset(EdgeDirection direction)
    {
        switch (direction)
        {
            case EdgeDirection.Up:
                return -10;
            case EdgeDirection.Down:
                return 10;
            case EdgeDirection.Left:
                return -1;
            case EdgeDirection.Right:
                return 1;
        }
        
        return 0;
    }
}
