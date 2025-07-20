using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;


public enum WallType
{
    None,
    Full,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    InnerTopLeft,
    InnerTopRight,
    InnerBottomLeft,
    InnerBottomRight,

    // Optional
    // Kreuzung
    T_Up,
    T_Down,
    T_Left,
    T_Right,

    Cross,
}

[System.Serializable]
public class WallTileMapping
{
    public WallType wallType;
    public TileBase tile;

    public WallTileMapping(WallType type, TileBase tile)
    {
        this.wallType = type;
        this.tile = tile;
    }
}

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

    [Tooltip("Anzahl der Versuche, um Räume zu generieren, bevor abgebrochen wird")]
    [SerializeField] private int maxGenerationAttempts = 100;


    [Tooltip("Anazahl der Gegner pro Ebene")]
    public int enemyCount = 5;

    [Tooltip("Anzahl der Items pro Ebene")]
    public int itemCount = 10;

    [Header("Smart Wall Tiles")]
    [SerializeField]
    private List<WallTileMapping> wallTileMappings = new List<WallTileMapping>();
    public Dictionary<WallType, TileBase> wallTiles = new Dictionary<WallType, TileBase>();

    public Room StartRoom { get; set; } // Start room for the player
    public Room ExitRoom { get; set; } // Exit room for the level

    public class Room
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

        Debug.Log("DungeonGenerator started.");
        // Generate the dungeon when the script starts
        // This can be called from the editor or at runtime.
        // You can also call this method from a button in the inspector.
        GenerateDungeon();
    }

    private float GetRoomDistance(Room roomA, Room roomB)
    {
        // Berechne den Abstand zwischen den Mittelpunkten der beiden Räume
        Vector2 centerA = roomA.center;
        Vector2 centerB = roomB.center;
        return Vector2.Distance(centerA, centerB);
    }

    private void FindStartAndEndRooms()
    {
        if (rooms.Count < 2)
        {
            Debug.LogWarning("Nicht genug Räume, um Start- und Endräume zu finden.");
            StartRoom = rooms.Count > 0 ? rooms[0] : null;
            ExitRoom = rooms.Count > 1 ? rooms[1] : null;
            return;
        }

        float maxDistance = 0f;
        Room currentStartRoom = null;
        Room currentEndRoom = null;

        // Iteriere über alle möglichen Raum-Paare
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++) // Beginnt bei i+1, um Duplikate und Selbstvergleiche zu vermeiden
            {
                Room room1 = rooms[i];
                Room room2 = rooms[j];

                float distance = GetRoomDistance(room1, room2);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    currentStartRoom = room1;
                    currentEndRoom = room2;
                }
            }
        }


        StartRoom = currentStartRoom;
        ExitRoom = currentEndRoom;

        if (StartRoom is not null && ExitRoom is not null)
        {
            Debug.Log($"Startraum gefunden bei: {StartRoom.bounds.center}");
            Debug.Log($"Endraum gefunden bei: {ExitRoom.bounds.center}");

        }
    }

    void Awake()
    {
        // Lösche das Dictionary bei jedem Start, um alte Einträge zu vermeiden
        wallTiles.Clear();

        // Initialisiere das Dictionary aus der serialisierten Liste
        foreach (var mapping in wallTileMappings)
        {
            // Prüfe, ob das Tile zugewiesen ist UND ob der Key noch nicht existiert
            if (mapping.tile != null && !wallTiles.ContainsKey(mapping.wallType))
            {
                wallTiles.Add(mapping.wallType, mapping.tile);
            }
            else if (mapping.tile == null)
            {
                Debug.LogWarning($"WallTileMapping für {mapping.wallType} hat kein Tile zugewiesen im Inspector!");
            }
            else if (wallTiles.ContainsKey(mapping.wallType))
            {
                Debug.LogWarning($"Doppelter WallType-Eintrag für {mapping.wallType} im Inspector. Nur der erste wird verwendet.");
            }
        }

        if (wallTiles.Count == 0)
        {
            Debug.LogError("Keine Wand-Tile-Mappings zugewiesen! Dungeon kann nicht korrekt gezeichnet werden.");
        }
        else
        {
            Debug.Log($"DungeonGenerator initialisiert mit {wallTiles.Count} Wand-Tile-Mappings.");
        }
    }

    [ContextMenu("Generate Dungeon")]
    void GenerateDungeon()
    {
        Awake(); // Initialize wall tiles
        Debug.Log("Generating dungeon...");
        // Validate inputs
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

        FindStartAndEndRooms();
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
        BoundsInt dungeonBounds = GetDungeonBounds();

        for (int x = dungeonBounds.xMin - 2; x < dungeonBounds.xMax + 2; x++) // Etwas größeren Bereich prüfen
        {
            for (int y = dungeonBounds.yMin - 2; y < dungeonBounds.yMax + 2; y++) // Etwas größeren Bereich prüfen
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                // Ist an dieser Position bereits Boden? Dann ist es keine Wand.
                if (groundTilemap.GetTile(pos) != null)
                {
                    continue; // Überspringe, da hier Boden ist
                }

                // Überprüfe die umliegenden Zellen, um den Wandtyp zu bestimmen
                WallType type = GetWallType(pos);
                //Debug.Log($"Position: {pos}, WallType: {type}");

                // Nur zeichnen, wenn ein passender Wandtyp gefunden wurde (d.h. es ist wirklich eine Wand)
                if (type != WallType.None && wallTiles.ContainsKey(type))
                {
                    wallTilemap.SetTile(pos, wallTiles[type]);
                }
                else if (type != WallType.None && !wallTiles.ContainsKey(type))
                {
                    // Fallback: Wenn wir einen Typ identifiziert haben, aber kein Tile dafür zugewiesen ist
                    Debug.LogWarning($"Kein Tile für WallType: {type} zugewiesen!");
                    if (wallTiles.ContainsKey(WallType.Full)) // Optional: Fallback auf ein Standard-Wand-Tile
                    {
                        wallTilemap.SetTile(pos, wallTiles[WallType.Full]);
                    }
                }
            }
        }
    }

    // Helper-Funktion, um zu prüfen, ob eine Zelle Boden istBottomLeft
    private bool IsGround(Vector3Int position)
    {
        return groundTilemap.GetTile(position) != null;
    }

    private WallType GetWallType(Vector3Int position)
    {
        // Status der direkten Nachbarn
        bool top = IsGround(position + new Vector3Int(0, 1, 0));
        bool bottom = IsGround(position + new Vector3Int(0, -1, 0));
        bool left = IsGround(position + new Vector3Int(-1, 0, 0));
        bool right = IsGround(position + new Vector3Int(1, 0, 0));

        // Status der diagonalen Nachbarn
        bool topLeft = IsGround(position + new Vector3Int(-1, 1, 0));
        bool topRight = IsGround(position + new Vector3Int(1, 1, 0));
        bool bottomLeft = IsGround(position + new Vector3Int(-1, -1, 0));
        bool bottomRight = IsGround(position + new Vector3Int(1, -1, 0));

        // --- Priorität 1: Innere Ecken (konkave Ecken) ---
        // Dies sind Fälle, wo der Boden in die Wand "einspringt"
        // Beispiel: InnerBottomRight -> Oben und Links ist Boden, aber oben-links ist KEIN Boden
        if (top && left && !topLeft && !bottom && !right) return WallType.InnerBottomRight;
        if (top && right && !topRight && !bottom && !left) return WallType.InnerBottomLeft;
        if (bottom && left && !bottomLeft && !top && !right) return WallType.InnerTopRight;
        if (bottom && right && !bottomRight && !top && !left) return WallType.InnerTopLeft;

        // --- Priorität 2: Äußere Ecken (konvexe Ecken) ---
        // Beispiel: BottomLeft -> Oben und Rechts ist Boden
        if (top && right && !bottom && !left) return WallType.BottomLeft; // Wall is Bottom-Left of ground area
        if (top && left && !bottom && !right) return WallType.BottomRight; // Wall is Bottom-Right of ground area
        if (bottom && right && !top && !left) return WallType.TopLeft; // Wall is Top-Left of ground area
        if (bottom && left && !top && !right) return WallType.TopRight; // Wall is Top-Right of ground area

        // --- Priorität 3: Geraden Kanten / Einzelne Seiten ---
        // Beispiel: Top -> Unten ist Boden
        if (top && !bottom && !left && !right) return WallType.Bottom; // Wall is below ground
        if (bottom && !top && !left && !right) return WallType.Top; // Wall is above ground
        if (left && !right && !top && !bottom) return WallType.Right; // Wall is right of ground
        if (right && !left && !top && !bottom) return WallType.Left; // Wall is left of ground

        // --- Priorität 4: Geraden Wände (umgeben von Boden auf zwei gegenüberliegenden Seiten) ---
        if (top && bottom && !left && !right) return WallType.Full; // Vertikale Wand
        if (left && right && !top && !bottom) return WallType.Full; // Horizontale Wand

        // --- Priorität 5: T-Kreuzungen (wenn du diese Tiles hast) ---
        if (top && bottom && left && !right) return WallType.T_Right;
        if (top && bottom && right && !left) return WallType.T_Left;
        if (top && left && right && !bottom) return WallType.T_Down;
        if (bottom && left && right && !top) return WallType.T_Up;

        // --- Priorität 6: Kreuzung (wenn alle 4 Seiten Boden sind, aber die Zelle selbst keine ist) ---
        if (top && bottom && left && right)
        {
            // Könnte eine Kreuzung sein, oder auch nur ein überflüssiger Wandversuch
            // Wenn du ein spezielles Kreuzungs-Tile hast, hier verwenden, sonst None
            return WallType.Cross; // Wenn du ein solches Tile hast
        }

        // --- Fallback: Wenn KEINER der Nachbarn Boden ist, dann ist es auch keine Wand. ---
        // Dies fängt Zellen ab, die weit außerhalb des Dungeons liegen.
        if (!top && !bottom && !left && !right && !topLeft && !topRight && !bottomLeft && !bottomRight)
        {
            return WallType.None;
        }

        // Letzter Fallback: Wenn wir hier ankommen, haben wir eine Kombination von Nachbarn,
        // die keinem der spezifischen WallTypes oben zugeordnet werden konnten.
        // In diesem Fall kannst du ein generisches "Full" Tile verwenden oder es als "None" behandeln.
        // Ein "Full" Tile ist hier oft eine gute Notlösung.
        return WallType.Full;
    }

    private BoundsInt GetDungeonBounds()
    {
        if (rooms.Count == 0)
        {
            return new BoundsInt(0, 0, 0, 1, 1, 1); // Rückgabe eines leeren BoundsInt, wenn keine Räume vorhanden sind
        }

        int minX = rooms.Min(r => r.bounds.xMin);
        int minY = rooms.Min(r => r.bounds.yMin);
        int maxX = rooms.Max(r => r.bounds.xMax);
        int maxY = rooms.Max(r => r.bounds.yMax);

        return new BoundsInt(minX, minY, 0, maxX - minX, maxY - minY, 1);
    }



    // Update is called once per frame
    void Update()
    {
    }
}