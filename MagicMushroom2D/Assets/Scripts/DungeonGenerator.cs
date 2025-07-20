using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public class DungeonGenerator : MonoBehaviour
{
    [Header("Tilemaps and Tiles")]
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;
    public TileBase groundTile;
    public TileBase wallTile;

    [Header("Prefabs für Entitäten")] public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject itemPrefab;
    public GameObject exitPrefab;

    [Header("Generator-Settings")]
    [Tooltip("Breite der generierten Ebene in Tiles")]
    public int levelWidth = 100;

    [Tooltip("Höhe der generierten Ebene in Tiles")]
    public int levelHeight = 100;

    [Tooltip("Maximale Anzahl von Räumen pro Ebene")]
    public int numberOfRooms = 20;

    [Tooltip("Minimale Raumgröße")] public int minRoomSize = 5;
    [Tooltip("Maximale Raumgröße")] public int maxRoomSize = 12;

    [SerializeField] private int maxGenerationAttempts = 100;


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
        public RectInt bounds { get; private set; }
        public Vector2 center => bounds.center;

        public Room(int x, int y, int width, int height)
        {
            bounds = new RectInt(x, y, width, height);
        }

        public bool Overlaps(Room other)
        {
            RectInt bufferedBounds = new RectInt(bounds.xMin - 1, bounds.yMin - 1, bounds.width + 2, bounds.height + 2);
            return bufferedBounds.Overlaps(other.bounds);
        }
    }

    private List<Room> rooms = new List<Room>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    void GenerateDungeon()
    {
        if (groundTilemap == null || wallTilemap == null || groundTile == null || wallTile == null)
        {
            Debug.LogError("Tilemaps or Tiles are not assigned.");
            return;
        }
        if (levelWidth <= 0 || levelHeight <= 0)
        {
            Debug.LogError("Level width and height must be greater than zero.");
            return;
        }
        if (numberOfRooms <= 0 || minRoomSize <= 0 || maxRoomSize < minRoomSize)
        {
            Debug.LogError("Invalid room settings.");
            return;
        }
        if (enemyCount < 0 || itemCount < 0)
        {
            Debug.LogError("Enemy and item counts must be non-negative.");
            return;
        }
        // Clear previous tiles and rooms
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        rooms.Clear();

        PlaceRooms();

        ConnectRooms();

        DrawTiles();

        return; // Temporarily disable dungeon generation for testing
        levelGrid = new MapCellType[levelWidth, levelHeight];
        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                levelGrid[x, y] = MapCellType.Wall;
            }
        }

        rooms.Clear();

        for (int i = 0; i < numberOfRooms; i++)
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

    private void PlaceRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            bool roomPlaced = false;
            int attempts = 0;
            while (!roomPlaced && attempts < maxGenerationAttempts)
            {
                int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
                int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
                int roomX = Random.Range(1, levelWidth - roomWidth - 1);
                int roomY = Random.Range(1, levelHeight - roomHeight - 1);
                Room newRoom = new Room(roomX, roomY, roomWidth, roomHeight);
                bool overlaps = false;
                foreach (var existingRoom in rooms)
                {
                    if (newRoom.Overlaps(existingRoom))
                    {
                        overlaps = true;
                        break;
                    }
                }
                if (!overlaps)
                {
                    rooms.Add(newRoom);
                    roomPlaced = true;

                }

                attempts++;
            }

            if (!roomPlaced)
            {
                Debug.LogWarning($"Could not place room {i + 1} after {maxGenerationAttempts} attempts.");
            }
        }
    }

    private void ConnectRooms()
    {
        if (rooms.Count < 2)
        {
            Debug.LogWarning("Not enough rooms to connect.");
            return;
        }

        List<Room> sortedRooms = rooms.OrderBy(r => r.bounds.center.x).ThenBy(r => r.bounds.center.y).ToList();

        for (int i = 0; i < sortedRooms.Count - 1; i++)
        {
            Room roomA = sortedRooms[i];
            Room roomB = sortedRooms[i + 1];

            Vector2Int startPoint = new Vector2Int(Random.Range(roomA.bounds.xMin + 1, roomA.bounds.xMax - 1), Random.Range(roomA.bounds.yMin + 1, roomA.bounds.yMax - 1));
            Vector2Int endPoint = new Vector2Int(Random.Range(roomB.bounds.xMin + 1, roomB.bounds.xMax - 1), Random.Range(roomB.bounds.yMin + 1, roomB.bounds.yMax - 1));

            DrawCorridor(startPoint, new Vector2Int(endPoint.x, startPoint.y));
            DrawCorridor(new Vector2Int(endPoint.x, startPoint.y), endPoint);

            /* 
                                                                        if (start.x == end.x)
                                                                        {
                                                                            // Vertical connection
                                                                            for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                                                                            {
                                                                                levelGrid[start.x, y] = MapCellType.Floor;
                                                                            }
                                                                        }
                                                                        else if (start.y == end.y)
                                                                        {
                                                                            // Horizontal connection
                                                                            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
                                                                            {
                                                                                levelGrid[x, start.y] = MapCellType.Floor;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            // Diagonal connection
                                                                            int dx = end.x - start.x;
                                                                            int dy = end.y - start.y;
                                                                            int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                                                                            float xIncrement = dx / (float)steps;
                                                                            float yIncrement = dy / (float)steps;

                                                                            for (int step = 0; step <= steps; step++)
                                                                            {
                                                                                int x = Mathf.RoundToInt(start.x + step * xIncrement);
                                                                                int y = Mathf.RoundToInt(start.y + step * yIncrement);
                                                                                levelGrid[x, y] = MapCellType.Floor;
                                                                            }
                                                                        } */
        }
    }

    private void DrawCorridor(Vector2Int startPoint, Vector2Int endPoint)
    {
        // Horizontaler Teil
        for (int x = Mathf.Min(startPoint.x, endPoint.x); x <= Mathf.Max(startPoint.x, endPoint.x); x++)
        {
            groundTilemap.SetTile(new Vector3Int(x, startPoint.y, 0), groundTile);
        }

        // Vertikaler Teil
        for (int y = Mathf.Min(startPoint.y, endPoint.y); y <= Mathf.Max(startPoint.y, endPoint.y); y++)
        {
            groundTilemap.SetTile(new Vector3Int(endPoint.x, y, 0), groundTile);
        }
    }


    private void DrawTiles()
    {
        foreach (Room room in rooms)
        {
            for (int x = room.bounds.x; x < room.bounds.x + room.bounds.width; x++)
            {
                for (int y = room.bounds.y; y < room.bounds.y + room.bounds.height; y++)
                {
                    groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                }
            }
        }

        // Jetzt die Wände zeichnen. Wir müssen um die Räume und Gänge herum zeichnen.
        // Eine einfache Methode ist es, jede gefüllte Boden-Kachel zu überprüfen und wenn eine
        // ihrer Nachbarkacheln leer ist, dort eine Wand zu platzieren.

        //Bounds _bounds = groundTilemap.localBounds; // Holen Sie sich die Grenzen der Boden-Tilemap
        BoundsInt bounds = groundTilemap.cellBounds; // Holen Sie sich die Grenzen der Boden-Tilemap in Ganzzahlen
        for (int x = bounds.xMin - 1; x < bounds.xMax + 1; x++)
        {
            for (int y = bounds.yMin - 1; y < bounds.yMax + 1; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                // Wenn die aktuelle Position leer ist UND eine benachbarte Zelle Boden enthält, ist es eine Wand
                if (groundTilemap.GetTile(pos) == null) // Position ist leer (kein Boden)
                {
                    // Überprüfe die 8 umliegenden Zellen + die eigene Zelle
                    bool hasAdjacentGround = false;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            // Überspringe die aktuelle Zelle selbst
                            if (dx == 0 && dy == 0) continue;

                            if (groundTilemap.GetTile(pos + new Vector3Int(dx, dy, 0)) != null)
                            {
                                hasAdjacentGround = true;
                                break;
                            }
                        }
                        if (hasAdjacentGround) break;
                    }

                    if (hasAdjacentGround)
                    {
                        wallTilemap.SetTile(pos, wallTile);
                    }
                }
            }
        }
    }

    void RenderTiles()
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < levelWidth; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (levelGrid[x, y] == MapCellType.Floor)
                {
                    groundTilemap.SetTile(cellPosition, groundTile);
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