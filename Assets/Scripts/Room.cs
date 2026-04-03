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

    [SerializeField] private Vector2 doorPosUp;
    [SerializeField] private Vector2 doorPosDown;
    [SerializeField] private Vector2 doorPosLeft;
    [SerializeField] private Vector2 doorPosRight;

    public void SetupRoom( Cell currentCell, RoomScriptable room )
    {
        spriteRenderer.sprite = room.roomVariations[ Random.Range( 0, room.roomVariations.Length ) ];

        if( currentCell.roomType == RoomType.Secret ) { return; }

        var floorPlan = MapGenerator.instance.getFloorPlan;
        var cellList = MapGenerator.instance.getSpawnedCells;

        SetupOneByOne( currentCell, floorPlan, cellList );
    }

    public void SetupOneByOne( Cell cell, Dictionary<Vector2Int, Cell> floorPlan, List<Cell> cellList )
    {
        Vector2Int currentCell = cell.key;

        TryPlaceDoor( currentCell, doorPosUp, EdgeDirection.Up, floorPlan, cellList, cell );
        TryPlaceDoor( currentCell, doorPosDown, EdgeDirection.Down, floorPlan, cellList, cell );
        TryPlaceDoor( currentCell, doorPosLeft, EdgeDirection.Left, floorPlan, cellList, cell );
        TryPlaceDoor( currentCell, doorPosRight, EdgeDirection.Right, floorPlan, cellList, cell );
    }

    private void TryPlaceDoor( Vector2Int fromKey, Vector2 positionOffset, EdgeDirection direction, Dictionary<Vector2Int, Cell> floorPlan, List<Cell> cellList, Cell currentCell )
    {
        Vector2Int neighbourKey = fromKey + GetOffset( direction );

        if (!floorPlan.ContainsKey( neighbourKey ) ) { return; }

        var foundCell = floorPlan[ neighbourKey ];

        if ( foundCell.roomType == RoomType.Secret ) { return; }

        var door = Instantiate( RoomManager.instance.doorPrefab, transform );
        door.transform.position = ( Vector2 )transform.position + positionOffset;

        SetupDoor( door, direction, currentCell.roomType == RoomType.Regular ? foundCell.roomType : currentCell.roomType );
    }

    private void SetupDoor( Door door, EdgeDirection direction, RoomType roomType )
    {
        var doorType = GetDoorOptions( roomType );
        door.SetDoorSprite( doorType.upDoor );

        switch ( direction )
        {
            case EdgeDirection.Up:
                door.direction = EdgeDirection.Up;
                break;
            case EdgeDirection.Down:
                door.direction = EdgeDirection.Down;
                door.GetComponent<BoxCollider>().transform.Rotate( 0, 0, 180 );
                break;
            case EdgeDirection.Left:
                door.direction = EdgeDirection.Left;
                door.GetComponent<BoxCollider>().transform.Rotate( 0, 0, 90 );
                break;
            case EdgeDirection.Right:
                door.direction = EdgeDirection.Right;
                door.GetComponent<BoxCollider>().transform.Rotate( 0, 0, -90 );
                break;
            default:
                break;

        }
    }

    private DoorScriptable GetDoorOptions( RoomType roomType )
    { 
        return RoomManager.instance.doors.FirstOrDefault( x=>x.roomType == roomType ); 
    }

    private Vector2Int GetOffset( EdgeDirection direction )
    {
        switch ( direction )
        {
            case EdgeDirection.Up:
                return new Vector2Int( 0, -1 );
            case EdgeDirection.Down:
                return new Vector2Int( 0, 1 );
            case EdgeDirection.Left:
                return new Vector2Int( -1, 0 );
            case EdgeDirection.Right:
                return new Vector2Int( 1, 0 );
        }
        
        return new Vector2Int( 0, 0 );
    }
}
