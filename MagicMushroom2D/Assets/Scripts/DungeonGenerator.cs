using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class DungeonGenerator : MonoBehaviour
{
    [Header("Tilemaps and Tiles")] public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Prefabs für Entitäten")] public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject itemPrefab;
    public GameObject exitPrefab;

    [Header("Generator-Settings")] [Tooltip("Breite der generierten Ebene in Tiles")]
    public int levelWidth = 60;

    [Tooltip("Höhe der generierten Ebene in Tiles")]
    public int levelHeight = 60;

    [Tooltip("Maximale Anzahl von Räumen pro Ebene")]
    public int maxRooms = 12;

    [Tooltip("Minimale Raumgröße")] public int minRoomSize = 6;
    [Tooltip("Maximale Raumgröße")] public int maxRoomSize = 12;

    [Tooltip("Anazahl der Gegner pro Ebene")]
    public int enemyCount = 5;

    [Tooltip("Anzahl der Items pro Ebene")]
    public int itemCount = 10;

    private enum MapCellType
    {
        Empty,
        Wall,
        Floor
    }

    private MapCellType[,] levelGrid;

    private struct Room
    {
        public RectInt bounds;
        public Vector2 center => bounds.center;

        public Room(int x, int y, int width, int height)
        {
            bounds = new RectInt(x, y, width, height);
        }
    }

    private List<Room> rooms = new List<Room>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        levelGrid = new MapCellType[levelWidth, levelHeight];
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                levelGrid[x, y] = MapCellType.Wall;
            }
        }

        rooms.Clear();

        for (int i = 0; i < maxRooms; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomX = Random.Range(1, levelWidth - roomWidth - 1);
            int roomY = Random.Range(1, levelHeight - roomHeight - 1);

            Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight);
            bool intersects = false;

            foreach (var existingRoom in rooms)
            {
                if (newRoom.bounds.Overlaps(existingRoom.bounds))
                {
                    intersects = true;
                    break;
                }
            }

            if (!intersects)
            {
                rooms.Add(newRoom);
                CarveRoom(newRoom);
            }
        }

        if (rooms.Count == 0)
        {
            Debug.LogError("No rooms generated");
            return;
        }

        rooms = rooms.OrderBy(room => room.center.x).ThenBy(r => r.center.y).ToList();

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            // Connect rooms here
        }
        
        // SpawnEntities();

        RenderTiles();
    }

    void RenderTiles()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (levelGrid[x, y] == MapCellType.Floor)
                {
                    floorTilemap.SetTile(cellPosition, floorTile);
                }
                else if (levelGrid[x, y] == MapCellType.Wall)
                {
                    wallTilemap.SetTile(cellPosition, wallTile);
                }
            }
        }
    }

    void CarveRoom(Room newRoom)
    {
        for (int x = 0; x < newRoom.bounds.x + newRoom.bounds.width; x++)
        {
            for (int y = 0; y < newRoom.bounds.y + newRoom.bounds.height; y++)
            {
                levelGrid[x, y] = MapCellType.Floor;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}