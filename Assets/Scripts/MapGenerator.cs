using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    public Dictionary<Vector2Int, Cell> newFloorPlan { get; private set; }
    public Dictionary<Vector2Int, Cell> floorPlan { get; private set; }
    public Dictionary<Vector2Int, Cell> getFloorPlan => newFloorPlan;
    public List<Cell> getSpawnedCells => newSpawnedCells;
    
    public static MapGenerator instance;

    [Header( "Generator Settings" )]
    [SerializeField] private int minRooms = 7;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int startLocationLegacy = 45;
    [SerializeField] private Vector2Int startLocation = new Vector2Int( 0, 0 );
    [SerializeField] private float endGenerationChance = 0.5f;
    [SerializeField] Cell cellPrefab;
    [SerializeField] private float cellSize = 0.5f;

    [Header( "Sprite References" )]
    [SerializeField] private Sprite item;
    [SerializeField] private Sprite shop;
    [SerializeField] private Sprite boss;
    [SerializeField] private Sprite secret;

    [Header( "Debug Settings" )]
    [SerializeField] private InputActionReference generateDungeon;
    [SerializeField] private InputActionReference extendDungeon;

    private int floorPlanCount = 0;
    private List<Vector2Int> endRooms;

    private Vector2Int bossRoomKey;
    private Vector2Int secretRoomKey;
    private Vector2Int shopRoomKey;
    private Vector2Int itemRoomKey;
    
    private Queue<int> cellQueueLegacy;
    private Queue<Vector2Int> cellQueue;
    private List<Cell> newSpawnedCells = new();
    private List<Cell> spawnedCells = new();

    private int stackOverflowLimiter = 0;

    private void Awake()
    {
        instance = this;
        newFloorPlan = new Dictionary<Vector2Int, Cell>();
        floorPlan = new Dictionary<Vector2Int, Cell>();
    }

    private void OnEnable()
    {
        generateDungeon.action.started += GenerateNewDungeon;
        extendDungeon.action.started += StartDungeonExtension;
    }

    private void OnDisable()
    {
        generateDungeon.action.started -= GenerateNewDungeon;
        extendDungeon.action.started -= StartDungeonExtension;
    }

    private void Start()
    {
        SetupDungeon();
    }

    private void StartDungeonExtension( InputAction.CallbackContext context )
    {
        SetupDungeonExtension();
    }

    private void GenerateNewDungeon( InputAction.CallbackContext context )
    {
        foreach ( var cell in floorPlan.Values.ToList() )
        {
            Destroy( cell.gameObject );
        }
        floorPlan.Clear();

        foreach ( var cell in spawnedCells.ToList() )
        { 
            Destroy( cell.gameObject );
        }
        spawnedCells.Clear();
        stackOverflowLimiter = default;
        
        SetupDungeon();
    }

    private void SetupDungeon()
    {
        newFloorPlan.Clear();
        newSpawnedCells.Clear();

        foreach ( var cell in floorPlan )
        {
            newFloorPlan.Add( cell.Key, cell.Value );
        }

        foreach ( var cell in spawnedCells )
        {
            newSpawnedCells.Add( cell );
        }

        cellQueue = new Queue<Vector2Int>();
        endRooms = new List<Vector2Int>();
        floorPlanCount = default;

        VisitCell( startLocation );

        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        while ( cellQueue.Count > 0 )
        {
            Vector2Int key = cellQueue.Dequeue();
            int x = key.x;
            int y = key.y;

            bool created = false;

            created |= VisitCell( ReturnNewVector( key, EdgeDirection.Left ) );
            created |= VisitCell( ReturnNewVector( key, EdgeDirection.Right ) );
            created |= VisitCell( ReturnNewVector( key, EdgeDirection.Up ) );
            created |= VisitCell( ReturnNewVector( key, EdgeDirection.Down ) );

            if ( created == false ) { endRooms.Add( key ); }
        }

        if ( floorPlanCount < minRooms )
        {
            Debug.Log( "Rooms generated this failed attempt: " + floorPlanCount.ToString() );
            if ( stackOverflowLimiter > 20 ) { Debug.LogError( "Generation failed, too many generation attempts" ); return; }
            stackOverflowLimiter++;
            SetupDungeon();
            return;
        }

        Debug.Log( "new floor plan count: " + newFloorPlan.Count );
        Debug.Log( "new spawned cells count: " + newSpawnedCells.Count );

        Debug.Log( "floor plan count: " + floorPlan.Count );
        Debug.Log( "spawned cells count: " + spawnedCells.Count );

        //Debug.Log( "Generation attempts: " + stackOverflowLimiter.ToString() );

        SetupSpecialRooms();
        foreach ( var cell in newFloorPlan )
        {
            if ( floorPlan.ContainsKey( cell.Key ) ) { continue; }
            floorPlan.Add( cell.Key, cell.Value );
        }

        foreach ( var cell in newSpawnedCells )
        {
            if ( spawnedCells.Contains(cell) ) { continue; }
            spawnedCells.Add( cell );
        }
    }

    private void SetupSpecialRooms()
    {
        bossRoomKey = endRooms.Count > 0 ? endRooms[endRooms.Count - 1] : startLocation;

        if ( bossRoomKey != startLocation )
        {
            endRooms.RemoveAt( endRooms.Count - 1 );
        }

        itemRoomKey = RandomEndRoom();
        shopRoomKey = RandomEndRoom();
        secretRoomKey = PickSecretRoom();

        if ( itemRoomKey == Vector2Int.zero || shopRoomKey == Vector2Int.zero || bossRoomKey == Vector2Int.zero || secretRoomKey == Vector2Int.zero )
        {
            SetupDungeon();
            return;
        }

        SpawnRoom( secretRoomKey );

        UpdateSpecialRoomVisuals();
        RoomManager.instance.SetupRooms( newSpawnedCells );
    }

    private void UpdateSpecialRoomVisuals()
    {
        foreach ( var cell in newSpawnedCells )
        {
            if ( cell.key == itemRoomKey )
            {
                cell.SetSpecialRoomSprite( item );
                cell.SetRoomType( RoomType.Item );
            }

            if ( cell.key == shopRoomKey )
            {
                cell.SetSpecialRoomSprite( shop );
                cell.SetRoomType( RoomType.Shop );
            }

            if ( cell.key == bossRoomKey )
            {
                cell.SetSpecialRoomSprite( boss );
                cell.SetRoomType( RoomType.Boss );
            }

            if ( cell.key == secretRoomKey )
            {
                cell.SetSpecialRoomSprite( secret );
                cell.SetRoomType( RoomType.Secret );
            }
        }
    }

    private Vector2Int RandomEndRoom()
    {
        if ( endRooms.Count == 0 ) return Vector2Int.zero;

        int randomRoom = Random.Range( 0, endRooms.Count );
        Vector2Int key = endRooms[ randomRoom ];

        endRooms.RemoveAt( randomRoom );
        return key;
    }

    private Vector2Int PickSecretRoom()
    {
        // check for 3 neighbors
        foreach ( var cell in newFloorPlan )
        {
            Vector2Int key = cell.Key;
            Vector2Int newKey = ReturnNewVector( key, EdgeDirection.Up );
            if ( !newFloorPlan.ContainsKey( newKey ) ) 
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 3 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Down );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 3 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Left );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 3 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Right );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 3 )
                {
                    return newKey;
                }
            }
        }

        // check for 2 neighbors
        foreach ( var cell in newFloorPlan )
        {
            Vector2Int key = cell.Key;
            Vector2Int newKey = ReturnNewVector( key, EdgeDirection.Up );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 2 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Down );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 2 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Left );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 2 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Right );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 2 )
                {
                    return newKey;
                }
            }
        }
        
        // check for 1 neighbor
        foreach ( var cell in newFloorPlan )
        {
            Vector2Int key = cell.Key;
            Vector2Int newKey = ReturnNewVector( key, EdgeDirection.Up );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 1 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Down );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 1 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Left );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 1 )
                {
                    return newKey;
                }
            }

            newKey = ReturnNewVector( key, EdgeDirection.Right );
            if ( !newFloorPlan.ContainsKey( newKey ) )
            {
                int NBC = GetNeighbourCount( newKey );
                if ( NBC >= 1 )
                {
                    return newKey;
                }
            }
        }

        return Vector2Int.zero;
    }

    private int GetNeighbourCount( Vector2Int key )
    {
        int temp = 0;
        if ( newFloorPlan.ContainsKey( ReturnNewVector( key, EdgeDirection.Up ) ) )
        {
            temp++;
        }
        if ( newFloorPlan.ContainsKey( ReturnNewVector( key, EdgeDirection.Down ) ) )
        {
            temp++;
        }
        if ( newFloorPlan.ContainsKey( ReturnNewVector( key, EdgeDirection.Left ) ) )
        {
            temp++;
        }
        if ( newFloorPlan.ContainsKey( ReturnNewVector( key, EdgeDirection.Right ) ) )
        {
            temp++;
        }

        return temp;
    }

    private bool VisitCell( Vector2Int key )
    {
        // Check if a cell is allowed to be spawned
        if ( newFloorPlan.ContainsKey( key ) ) { return false; }
        if ( key != startLocation )
        {
            if ( GetNeighbourCount( key ) > 1 ) { return false; }
        }
        if ( floorPlanCount >= maxRooms ) { return false; }
        if ( Random.value < endGenerationChance ) { return false; }

        newFloorPlan.Add( key, null );
        cellQueue.Enqueue( key );
        floorPlanCount++;

        SpawnRoom( key );

        return true;
    }

    private void SpawnRoom( Vector2Int key )
    {
        int x = key.x;
        int y = key.y;
        Vector2 position = new Vector2( ( x * cellSize ), ( -y * cellSize ) );

        Cell newCell = Instantiate( cellPrefab, position, Quaternion.identity );
        newCell.value = 1;
        newCell.key = key;
        newCell.SetRoomType( RoomType.Regular );

        newFloorPlan[ key ] = newCell;
        newCell.neighborCellList.Add( key );

        newSpawnedCells.Add( newCell );
    }

    private void SetupDungeonExtension()
    {
        startLocation = GetExtensionStartLocation( bossRoomKey );
        bossRoomKey = default;
        shopRoomKey = default;
        itemRoomKey = default;
        secretRoomKey = default;
        endRooms.Clear();
        SetupDungeon();
    }

    private Vector2Int GetExtensionStartLocation(Vector2Int bossKey)
    {
        Vector2Int tempKey;

        tempKey = ReturnNewVector( bossKey, EdgeDirection.Up );
        if ( floorPlan.ContainsKey( tempKey ) ) 
        {
            Debug.Log( "Found neighbor above bossroom" );
            return ReturnNewVector( bossKey, EdgeDirection.Down ); 
        }

        tempKey = ReturnNewVector( bossKey, EdgeDirection.Down );
        if ( floorPlan.ContainsKey( tempKey ) ) 
        {
            Debug.Log( "Found neighbor Below bossroom" );
            return ReturnNewVector( bossKey, EdgeDirection.Up ); 
        }

        tempKey = ReturnNewVector( bossKey, EdgeDirection.Left );
        if ( floorPlan.ContainsKey( tempKey ) ) 
        {
            Debug.Log( "Found neighbor left of bossroom" );
            return ReturnNewVector( bossKey, EdgeDirection.Right ); 
        }

        tempKey = ReturnNewVector( bossKey, EdgeDirection.Right );
        if ( floorPlan.ContainsKey( tempKey ) ) 
        {
            Debug.Log( "Found neighbor right of bossroom" );
            return ReturnNewVector( bossKey, EdgeDirection.Left ); 
        }

        Debug.LogWarning( "Could not find a bossroom neighbor!" );
        return Vector2Int.zero ;
    }

    private Vector2Int ReturnNewVector( Vector2Int key, EdgeDirection dir )
    {
        switch ( dir )
        {
            case EdgeDirection.Up:
                return new Vector2Int( key.x, key.y + 1 );
            case EdgeDirection.Down:
                return new Vector2Int( key.x, key.y - 1 );
            case EdgeDirection.Left:
                return new Vector2Int( key.x - 1, key.y );
            case EdgeDirection.Right:
                return new Vector2Int( key.x + 1, key.y );
            default:
                return Vector2Int.zero;
        }
    }
}
