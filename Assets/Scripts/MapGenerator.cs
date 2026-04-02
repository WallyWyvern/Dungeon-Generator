using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    [SerializeField] private int minRooms = 7;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int startLocationLegacy = 45;
    [SerializeField] private Vector2Int startLocation = new Vector2Int(0,0);
    [SerializeField] private float endGenerationChance = 0.5f;
    [SerializeField] Cell cellPrefab;
    [SerializeField] private float cellSize = 0.5f;

    private int[] floorPlanLegacy;
    public Dictionary<Vector2Int, Cell> newFloorPlan { get; private set; }
    public Dictionary<Vector2Int, Cell> floorPlan { get; private set; }
    private int floorPlanCount;
    private List<int> endRoomsLegacy;
    private List<Vector2Int> endRooms;

    public Dictionary<Vector2Int, Cell> getFloorPlan => newFloorPlan;

    // legacy variables
    private int bossRoomIndex;
    private int secretRoomIndex;
    private int shopRoomIndex;
    private int itemRoomIndex;

    // new variables
    private Vector2Int bossRoomKey;
    private Vector2Int secretRoomKey;
    private Vector2Int shopRoomKey;
    private Vector2Int itemRoomKey;
    
    private float positionOffsetX;
    private float positionOffsetY;

    private Queue<int> cellQueueLegacy;
    private Queue<Vector2Int> cellQueue;
    private List<Cell> newSpawnedCells = new();
    private List<Cell> spawnedCells = new();

    public List<Cell> getSpawnedCells => spawnedCells;

    [Header("Sprite References")]
    [SerializeField] private Sprite item;
    [SerializeField] private Sprite shop;
    [SerializeField] private Sprite boss;
    [SerializeField] private Sprite secret;

    [Header("Debug Settings")]
    [SerializeField] private InputActionReference generateDungeon;
    [SerializeField] private InputActionReference extendDungeon;


    // infinite grid with dictionary (x,y naar int)? 

    /* references for dictionary refactor
        +1 is x + 1
        -1 is x - 1
        +10 is y - 1
        -10 is y + 1
     
        index is a Vector2Int (dictionary key)
        
     */
    public static MapGenerator instance;


    private void Awake()
    {
        instance = this;
        newFloorPlan = new Dictionary<Vector2Int, Cell>();
        floorPlan = new Dictionary<Vector2Int, Cell>();
    }

    private void OnEnable()
    {
        generateDungeon.action.started += generateNewDungeon;
        extendDungeon.action.started += startDungeonExtension;
    }

    private void OnDisable()
    {
        generateDungeon.action.started -= generateNewDungeon;
        extendDungeon.action.started -= startDungeonExtension;
    }
    private void startDungeonExtension(InputAction.CallbackContext context)
    {
        SetupDungeonExtension();
    }

    private void generateNewDungeon(InputAction.CallbackContext context)
    {
        Debug.Log("pressed space");

        foreach (var cell in floorPlan.Values.ToList())
        {
            Destroy(cell.gameObject);
        }
        floorPlan.Clear();

        foreach (var cell in spawnedCells.ToList())
        { 
            Destroy(cell.gameObject);
        }
        spawnedCells.Clear();

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

    void SetupDungeonLegacy() 
    {
        for (int i = 0; i < newSpawnedCells.Count; i++)
        {
            Destroy(newSpawnedCells[i].gameObject);
        }
        newSpawnedCells.Clear();

        floorPlanLegacy = new int[100];
        floorPlanCount = default;
        cellQueueLegacy = new Queue<int>();
        endRoomsLegacy = new List<int>();

        VisitCellLegacy(startLocationLegacy);

        GenerateDungeonLegacy();
    }

    void SetupDungeon()
    {
        foreach (var cell in newFloorPlan.Values.ToList())
        {
            Destroy(cell.gameObject);
        }
        newFloorPlan.Clear();

        foreach (var cell in newSpawnedCells.ToList())
        {
            Destroy(cell.gameObject);
        }
        newSpawnedCells.Clear();

        foreach (var cell in floorPlan)
        {
            newFloorPlan.Add(cell.Key, cell.Value);
        }

        foreach (var cell in spawnedCells)
        {
            newSpawnedCells.Add(cell);
        }

        cellQueue = new Queue<Vector2Int>();
        endRooms = new List<Vector2Int>();

        VisitCell(startLocation);

        GenerateDungeon();
    }

    void GenerateDungeonLegacy() 
    {
        while (cellQueueLegacy.Count > 0)
        { 
            int index = cellQueueLegacy.Dequeue();
            int x = index % 10;

            bool created = false;

            if (x > 1) { created |= VisitCellLegacy(index - 1); }
            if (x < 9) { created |= VisitCellLegacy(index + 1); }
            if (index > 20) { created |= VisitCellLegacy(index - 10); }
            if (index < 70) { created |= VisitCellLegacy(index + 10); }

            if (created == false) { endRoomsLegacy.Add(index); }
        }

        if(floorPlanCount < minRooms)
        { 
            SetupDungeonLegacy(); 
            return;
        }

        SetupSpecialRoomsLegacy();
    }

    void GenerateDungeon()
    {
        while (cellQueue.Count > 0)
        {
            Vector2Int key = cellQueue.Dequeue();
            int x = key.x;
            int y = key.y;

            bool created = false;

            created |= VisitCell(ReturnNewVector(key, EdgeDirection.Left));
            created |= VisitCell(ReturnNewVector(key, EdgeDirection.Right));
            created |= VisitCell(ReturnNewVector(key, EdgeDirection.Up));
            created |= VisitCell(ReturnNewVector(key, EdgeDirection.Down));

            if (created == false) { endRooms.Add(key); }
        }

        if (newFloorPlan.Count < minRooms)
        {
            SetupDungeon();
            return;
        }

        Debug.Log("new floor plan count: " + newFloorPlan.Count);
        Debug.Log("new spawned cells count: " + newSpawnedCells.Count);

        Debug.Log("floor plan count: " + floorPlan.Count);
        Debug.Log("spawned cells count: " + spawnedCells.Count);




        SetupSpecialRooms();
        foreach (var cell in newFloorPlan)
        {
            if (floorPlan.ContainsKey(cell.Key)) { continue; }
            floorPlan.Add(cell.Key, cell.Value);
        }

        foreach (var cell in newSpawnedCells)
        {
            if (spawnedCells.Contains(cell)) { continue; }
            spawnedCells.Add(cell);
        }
    }

    void SetupSpecialRoomsLegacy() 
    {
        bossRoomIndex = endRoomsLegacy.Count > 0 ? endRoomsLegacy[endRoomsLegacy.Count - 1] : -1;

        if (bossRoomIndex != -1)
        { 
            endRoomsLegacy.RemoveAt(endRoomsLegacy.Count - 1);
        }

        itemRoomIndex = RandomEndRoomLegacy();
        shopRoomIndex = RandomEndRoomLegacy();
        secretRoomIndex = PickSecretRoomLegacy();

        if (itemRoomIndex == -1 || shopRoomIndex == -1 || bossRoomIndex == -1 || secretRoomIndex == -1)
        {
            SetupDungeonLegacy();
            return;
        }

        SpawnRoomLegacy(secretRoomIndex);

        UpdateSpecialRoomVisualsLegacy();
        RoomManager.instance.SetupRooms(newSpawnedCells);
    }

    void SetupSpecialRooms()
    {
        bossRoomKey = endRooms.Count > 0 ? endRooms[endRooms.Count - 1] : startLocation;

        if (bossRoomKey != startLocation)
        {
            endRooms.RemoveAt(endRooms.Count - 1);
        }

        itemRoomKey = RandomEndRoom();
        shopRoomKey = RandomEndRoom();
        secretRoomKey = PickSecretRoom();

        if (itemRoomKey == Vector2Int.zero || shopRoomKey == Vector2Int.zero || bossRoomKey == Vector2Int.zero || secretRoomKey == Vector2Int.zero)
        {
            SetupDungeon();
            return;
        }

        SpawnRoom(secretRoomKey);

        UpdateSpecialRoomVisuals();
        RoomManager.instance.SetupRooms(newSpawnedCells);
    }

    void UpdateSpecialRoomVisualsLegacy() 
    {
        foreach (var cell in newSpawnedCells)
        { 
            if(cell.index == itemRoomIndex)
            {
                cell.SetSpecialRoomSprite(item);
                cell.SetRoomType(RoomType.Item);
            }

            if (cell.index == shopRoomIndex)
            {
                cell.SetSpecialRoomSprite(shop);
                cell.SetRoomType(RoomType.Shop);
            }

            if (cell.index == bossRoomIndex)
            {
                cell.SetSpecialRoomSprite(boss);
                cell.SetRoomType(RoomType.Boss);
            }

            if (cell.index == secretRoomIndex)
            {
                cell.SetSpecialRoomSprite(secret);
                cell.SetRoomType(RoomType.Secret);
            }
        }
    }

    void UpdateSpecialRoomVisuals()
    {
        foreach (var cell in newSpawnedCells)
        {
            if (cell.key == itemRoomKey)
            {
                cell.SetSpecialRoomSprite(item);
                cell.SetRoomType(RoomType.Item);
            }

            if (cell.key == shopRoomKey)
            {
                cell.SetSpecialRoomSprite(shop);
                cell.SetRoomType(RoomType.Shop);
            }

            if (cell.key == bossRoomKey)
            {
                cell.SetSpecialRoomSprite(boss);
                cell.SetRoomType(RoomType.Boss);
            }

            if (cell.key == secretRoomKey)
            {
                cell.SetSpecialRoomSprite(secret);
                cell.SetRoomType(RoomType.Secret);
            }
        }
    }

    int RandomEndRoomLegacy() 
    {
        if (endRoomsLegacy.Count == 0) return -1;

        int randomRoom = Random.Range(0, endRoomsLegacy.Count);
        int index = endRoomsLegacy[randomRoom];

        endRoomsLegacy.RemoveAt(randomRoom);
        return index;
    }

    Vector2Int RandomEndRoom()
    {
        if (endRooms.Count == 0) return Vector2Int.zero;

        int randomRoom = Random.Range(0, endRooms.Count);
        Vector2Int key = endRooms[randomRoom];

        endRooms.RemoveAt(randomRoom);
        return key;
    }

    int PickSecretRoomLegacy() 
    {
        for (int attempt = 0; attempt < 900; attempt++)
        { 
            int x = Mathf.FloorToInt(Random.Range(0f, 1f) * 9) + 1;
            int y = Mathf.FloorToInt(Random.Range(0f, 1f) * 8) + 2;

            int index = y * 10 + x;

            if (floorPlanLegacy[index] != 0)
            {
                continue;
            }

            if (bossRoomIndex == index - 1 || bossRoomIndex == index + 1 || bossRoomIndex == index - 10 || bossRoomIndex == index + 10)
            {
                continue;
            }

            if (index - 1 < 0 || index + 1 > floorPlanLegacy.Length || index - 10 < 0 || index + 10 > floorPlanLegacy.Length)
            {
                continue;
            }

            int neighbours = GetNeighbourCountLegacy(index);

            if (neighbours >= 3 || (attempt > 300 && neighbours >= 2) || (attempt > 600 && neighbours >= 1))
            {
                return index;
            }
        }

        return -1;
    }

    Vector2Int PickSecretRoom()
    {
        // check for 3 neighbors
        foreach (var cell in newFloorPlan)
        {
            Vector2Int key = cell.Key;
            Vector2Int newKey = ReturnNewVector(key, EdgeDirection.Up);
            if (!newFloorPlan.ContainsKey(newKey)) 
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 3)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Down);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 3)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Left);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 3)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Right);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 3)
                {
                    return newKey;
                }
            }
        }

        // check for 2 neighbors
        foreach (var cell in newFloorPlan)
        {
            Vector2Int key = cell.Key;
            Vector2Int newKey = ReturnNewVector(key, EdgeDirection.Up);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 2)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Down);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 2)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Left);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 2)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Right);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 2)
                {
                    return newKey;
                }
            }
        }
        
        // check for 1 neighbor
        foreach (var cell in newFloorPlan)
        {
            Vector2Int key = cell.Key;
            Vector2Int newKey = ReturnNewVector(key, EdgeDirection.Up);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 1)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Down);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 1)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Left);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 1)
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector(key, EdgeDirection.Right);
            if (!newFloorPlan.ContainsKey(newKey))
            {
                int NBC = GetNeighbourCount(newKey);
                if (NBC >= 1)
                {
                    return newKey;
                }
            }
        }

        return Vector2Int.zero;
    }

    private int GetNeighbourCountLegacy(int index) 
    {
        return floorPlanLegacy[index - 10] + floorPlanLegacy[index + 10] + floorPlanLegacy[index - 1] + floorPlanLegacy[index + 1];
    }

    private int GetNeighbourCount(Vector2Int key)
    {
        int temp = 0;
        if (newFloorPlan.ContainsKey(ReturnNewVector(key, EdgeDirection.Up)))
        {
            temp++;
        }
        if (newFloorPlan.ContainsKey(ReturnNewVector(key, EdgeDirection.Down)))
        {
            temp++;
        }
        if (newFloorPlan.ContainsKey(ReturnNewVector(key, EdgeDirection.Left)))
        {
            temp++;
        }
        if (newFloorPlan.ContainsKey(ReturnNewVector(key, EdgeDirection.Right)))
        {
            temp++;
        }

        return temp;
    }

    private bool VisitCellLegacy(int index) 
    {
        // Check if a cell is allowed to be spawned
        if (floorPlanLegacy[index] != 0) { return false; }
        if (GetNeighbourCountLegacy(index) > 1) { return false; }
        if (floorPlanCount > maxRooms) { return false; }
        if (Random.value < endGenerationChance) { return false; }

        cellQueueLegacy.Enqueue(index);
        floorPlanLegacy[index] = 1;
        floorPlanCount++;

        SpawnRoomLegacy(index);

        return true;
    }

    private bool VisitCell(Vector2Int key)
    {
        // Check if a cell is allowed to be spawned
        if (newFloorPlan.ContainsKey(key)) { return false; }
        if (GetNeighbourCount(key) > 1) { return false; }
        if (newFloorPlan.Count >= maxRooms) { return false; }
        if (Random.value < endGenerationChance) { return false; }

        newFloorPlan.Add(key, null);
        cellQueue.Enqueue(key);

        SpawnRoom(key);

        return true;
    }

    private void SpawnRoomLegacy(int index) 
    {
        int x = index % 10;
        int y = index / 10;
        Vector2 position = new Vector2((x * cellSize) + positionOffsetX, (-y * cellSize) + positionOffsetY);

        Cell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        newCell.value = 1;
        newCell.index = index;
        newCell.SetRoomType(RoomType.Regular);

        newCell.cellListLegacy.Add(index);

        newSpawnedCells.Add(newCell);
    }

    private void SpawnRoom(Vector2Int key)
    {
        int x = key.x;
        int y = key.y;
        Vector2 position = new Vector2((x * cellSize) + positionOffsetX, (-y * cellSize) + positionOffsetY);

        Cell newCell = Instantiate(cellPrefab, position, Quaternion.identity);
        newCell.value = 1;
        newCell.key = key;
        newCell.SetRoomType(RoomType.Regular);

        newFloorPlan[key] = newCell;
        newCell.neighborCellList.Add(key);

        newSpawnedCells.Add(newCell);
    }

    void SetupDungeonExtension()
    {
        floorPlanLegacy = new int[100];
        floorPlanCount = default;
        cellQueueLegacy = new Queue<int>();
        endRoomsLegacy = new List<int>();

        // get bossroom position
        Vector2 bossRoomPosition = GetBossRoomPosition(bossRoomIndex);
        Debug.Log(bossRoomPosition.ToString());
        // check boss room neighbour and calculate offsets
        if (floorPlanLegacy[bossRoomIndex + 1] == 1)
        {
            positionOffsetX += (9 - ((bossRoomIndex + 10) % 10)) * cellSize;
            positionOffsetY += (5 - ((bossRoomIndex + 10) / 10)) * cellSize;
            startLocationLegacy = 59;
        }
        if (floorPlanLegacy[bossRoomIndex - 1] == 1)
        {
            positionOffsetX += (((bossRoomIndex + 10) % 10)) * cellSize;
            positionOffsetY += (((bossRoomIndex + 10) / 10) - 5) * cellSize;
            startLocationLegacy = 50;
        }
        if (floorPlanLegacy[bossRoomIndex + 10] == 1)
        {
            positionOffsetX += (((bossRoomIndex - 10) % 10) - 5) * cellSize;
            positionOffsetY += (((bossRoomIndex - 10) / 10) - 9) * cellSize;
            startLocationLegacy = 95;
        }
        if (floorPlanLegacy[bossRoomIndex - 10] == 1)
        {
            positionOffsetX += (5 - ((bossRoomIndex + 10) % 10)) * cellSize;
            positionOffsetY += (((bossRoomIndex + 10) / 10)) * cellSize;
            startLocationLegacy = 5;
        }

        VisitCellLegacy(startLocationLegacy);

        GenerateDungeonLegacy();
    }

    Vector2 GetBossRoomPosition(int index)
    { 
        foreach(var cell in newSpawnedCells)
        {
            if (cell.index == index)
            {
                return cell.transform.position;
            }
        }
        return Vector2.zero;
    }

    private Vector2Int ReturnNewVector(Vector2Int key, EdgeDirection dir)
    {
        switch (dir)
        {
            case EdgeDirection.Up:
                return new Vector2Int(key.x, key.y + 1);
            case EdgeDirection.Down:
                return new Vector2Int(key.x, key.y - 1);
            case EdgeDirection.Left:
                return new Vector2Int(key.x - 1, key.y);
            case EdgeDirection.Right:
                return new Vector2Int(key.x + 1, key.y);
            default:
                return Vector2Int.zero;
        }
    }
}
