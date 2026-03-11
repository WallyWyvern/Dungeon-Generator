using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    [SerializeField] private int minRooms = 7;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int startLocation = 45;
    [SerializeField] private float endGenerationChance = 0.5f;
    [SerializeField] Cell cellPrefab;
    [SerializeField] private float cellSize = 0.5f;

    private int[] floorPlan;
    private int floorPlanCount;
    private List<int> endRooms;


    private int bossRoomIndex;
    private int secretRoomIndex;
    private int shopRoomIndex;
    private int itemRoomIndex;

    private Queue<int> cellQueue;
    private List<Cell> spawnedCells = new();

    [Header("Sprite References")]
    [SerializeField] private Sprite item;
    [SerializeField] private Sprite shop;
    [SerializeField] private Sprite boss;
    [SerializeField] private Sprite secret;

    [Header("Debug Settings")]
    [SerializeField] private InputActionReference generateDungeon;
    [SerializeField] private InputActionReference extendDungeon;


    private void OnEnable()
    {
        generateDungeon.action.started += generateNewDungeon;
    }

    private void OnDisable()
    {
        generateDungeon.action.started -= generateNewDungeon;
    }

    private void generateNewDungeon(InputAction.CallbackContext context)
    {
        Debug.Log("pressed space");
        SetupDungeon();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) { SetupDungeon(); }
    }

    void SetupDungeon() 
    {
        for (int i = 0; i < spawnedCells.Count; i++)
        {
            Destroy(spawnedCells[i].gameObject);
        }
        spawnedCells.Clear();

        floorPlan = new int[100];
        floorPlanCount = default;
        cellQueue = new Queue<int>();
        endRooms = new List<int>();

        VisitCell(startLocation);

        GenerateDungeon();
    }

    void GenerateDungeon() 
    {
        while (cellQueue.Count > 0)
        { 
            int index = cellQueue.Dequeue();
            int x = index % 10;

            bool created = false;

            if (x > 1) { created |= VisitCell(index - 1); }
            if (x < 9) { created |= VisitCell(index + 1); }
            if (index > 20) { created |= VisitCell(index - 10); }
            if (index < 70) { created |= VisitCell(index + 10); }

            if (created == false) { endRooms.Add(index); }
        }

        if(floorPlanCount < minRooms)
        { 
            SetupDungeon(); 
            return;
        }

        SetupSpecialRooms();
    }

    void SetupSpecialRooms() { }

    void UpdateSpecialRoomVisuals() { }

    int RandomEndRoom() 
    {
        return -1;
    }

    int PickSecretRoom() 
    {
        return -1;
    }

    private int GetNeighbourCount(int index) 
    {
        return floorPlan[index - 10] + floorPlan[index + 10] + floorPlan[index - 1] + floorPlan[index + 1];
    }

    private bool VisitCell(int index) 
    {
        // Check if a cell is allowed to be spawned
        if (floorPlan[index] != 0) { return false; }
        if (GetNeighbourCount(index) > 1) { return false; }
        if (floorPlanCount > maxRooms) { return false; }
        if (Random.value < endGenerationChance) { return false; }

        cellQueue.Enqueue(index);
        floorPlan[index] = 1;
        floorPlanCount++;

        SpawnRoom(index);

        return true;
    }

    private void SpawnRoom(int index) 
    {
        int x = index % 10;
        int y = index / 10;
        Vector2 position = new Vector2(x * cellSize, -y * cellSize);

        Cell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        newCell.value = 1;
        newCell.index = index;

        spawnedCells.Add(newCell);
    }
}
