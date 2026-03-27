using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private List<Room> createdRooms;

    [Header("Offset Variables")]
    public float xOffset;
    public float yOffset;

    [Header("Prefab References")]
    public Room roomPrefab;
    public Door doorPrefab;

    [Header("Scriptable Object References")]
    public DoorScriptable[] doors;
    public RoomScriptable[] rooms;

    public static RoomManager instance;

    private void Awake()
    {
        instance = this;
        createdRooms = new List<Room>();
    }

    public void SetupRooms(List<Cell> spawnedCells)
    {
        for(int i = createdRooms.Count - 1; i >= 0; i--)
        {
            Destroy(createdRooms[i].gameObject);
        }

        createdRooms.Clear();

        foreach(var currentCell in spawnedCells)
        {
            var foundRoom = rooms.FirstOrDefault(x=>x.roomType == currentCell.roomType && DoesTileMatchCell(x.occupiedTiles, currentCell));
            var currentPosition = currentCell.transform.position;
            var convertedPosition = new Vector2(currentPosition.x * xOffset, currentPosition.y * yOffset);
            var spawnedRoom = Instantiate(roomPrefab, convertedPosition, Quaternion.identity);

            spawnedRoom.SetupRoom(currentCell, foundRoom);

            createdRooms.Add(spawnedRoom);
        }
    }

    private bool DoesTileMatchCell(int[] occupiedTiles, Cell cell)
    {
        if(occupiedTiles.Length != cell.cellList.Count) return false;

        Vector2Int minKey = cell.cellList.Min();
        List<int> normalizedCell = new List<int>();

        foreach (Vector2Int key in cell.cellList)
        {
            int dx = (key.x) - (minKey.x);
            int dy = (key.y) - (minKey.y);

            normalizedCell.Add(dy * 10 + dx);
        }

        normalizedCell.Sort();
        int[] sortedOccupied = (int[])occupiedTiles.Clone();
        Array.Sort(sortedOccupied);

        return normalizedCell.SequenceEqual(sortedOccupied);
    }
}
