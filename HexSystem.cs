using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public class HexSystem : MonoBehaviour
{

    // selector
    private GameObject selector;
    private Vector3 selectorOffset = new Vector3(0, -0.13f, 0);

    // character
    private PlayerCharacter knightCharacter;
    private PlayerCharacter mageCharacter;
    private PlayerCharacter enemyCharacter;

    private PlayerCharacter activeCharacter;

    private List<PlayerCharacter> mapCharacters;

    // Grids and Tilemaps
    private Grid fullMap;
    private Tilemap tileOverlay;
    private Tilemap[] hexMaps;
    private TilemapRenderer[] hexMapRenderers;
    private HexType[] hexTypes;

    private int mapXCoord, mapYCoord;
    [SerializeField] private int mapSizeX, mapSizeY;
    private List<Vector3Int> walkableTileCoords;
    private Tile walkingOverlayTile;

    // possibly useless
    private Dictionary<Vector3Int, Tile> hexArray;
    private bool shouldRedraw;

    private TileUISystem tileUISystem;

    private BoundsInt mapArea;

    // might not need it any more
    private BoundsInt waterArea;
    private BoundsInt raisedLandArea;
    private BoundsInt landArea;
    private BoundsInt mountainArea;
    private BoundsInt treeArea;

    // Start is called before the first frame update
    void Start()
    {
        // find and allows TileSelector to be editable through the script
        selector = GameObject.Find("TileSelector");

        // find and initialize character
        knightCharacter = GameObject.Find("Knight").GetComponent<PlayerCharacter>();
        mageCharacter = GameObject.Find("Mage").GetComponent<PlayerCharacter>();
        enemyCharacter = GameObject.Find("Enemy").GetComponent<PlayerCharacter>();
        activeCharacter = knightCharacter;

        mapCharacters = new List<PlayerCharacter>();
        mapCharacters.Add(knightCharacter);
        mapCharacters.Add(mageCharacter);
        mapCharacters.Add(enemyCharacter);

        // initialize all Tilemaps within the level
        fullMap = transform.GetComponent<Grid>();
        hexMaps = transform.GetComponentsInChildren<Tilemap>();
        hexMapRenderers = transform.GetComponentsInChildren<TilemapRenderer>();
        hexArray = new Dictionary<Vector3Int, Tile>();

        tileOverlay = GameObject.Find("TileOverlay").GetComponent<Tilemap>();
        walkableTileCoords = new List<Vector3Int>();
        walkingOverlayTile = Tile.CreateInstance(typeof(Tile)) as Tile;
        walkingOverlayTile.sprite = Resources.Load<Sprite>("Sprites/tileHighlight");

        // initialize other systems
        tileUISystem = GameObject.Find("Canvas").GetComponentInChildren<TileUISystem>();

        // initialize tile UI components
        tileUISystem.SetTileTerrain(GetTileType(selector.transform.position), selector.transform.position);

        // get proper size of Hex Array and sets all the area bounds for each Tilemap
        foreach (Tilemap tilemap in hexMaps)
        {
            if (tilemap.cellBounds.size.x > mapSizeX)
            {
                mapSizeX = tilemap.cellBounds.size.x;
            }

            if (tilemap.cellBounds.size.y > mapSizeY)
            {
                mapSizeY = tilemap.cellBounds.size.y;
            }

            switch (tilemap.name)
            {
                case "Water":
                    waterArea = tilemap.cellBounds; break;
                case "RaisedLand":
                    raisedLandArea = tilemap.cellBounds; break;
                case "Land":
                    landArea = tilemap.cellBounds; break;
                case "Mountains":
                    mountainArea = tilemap.cellBounds; break;
                case "Trees:":
                    treeArea = tilemap.cellBounds; break;
            }

        }

        hexTypes = new HexType[hexMaps.Length];

        // Create hex array based on above
        foreach (Tilemap tilemap in hexMaps)
        {
            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(position))
                {
                    if (!hexArray.ContainsKey(position))
                    {
                        hexArray.Add(position, (Tile)tilemap.GetTile(position));
                    }
                }
            }
        }

        knightCharacter.SetStamina(3);
        CreateMovementOverlay(knightCharacter.GetStamina(), knightCharacter.GetPosition());

        Vector3 pointA = new Vector3(0, 0, 0);
        Vector3 pointB = new Vector3(2, 1, 0);

        BuildPath(pointA, pointB, 6);

        /************************************************
               
        foreach (KeyValuePair<Vector3Int, Tile> kvp in hexArray)
        {
            // Debug.Log(string.Format("Key = {0}, Value = {1}", kvp.Key, kvp.Value));
        }


        foreach (TilemapRenderer renderer in hexMapRenderers)
        {
            Debug.Log(renderer.gameObject + ", " + renderer.sortingOrder);
        }
        
        
        // Create hex array based on above
        for (int i = 0; i < transform.childCount; i++)
        {
            hexTypes[i] = new HexType();
            hexTypes[i].HexTypeName = transform.GetChild(i).name;
        }

        for (int i = 0; i < mapSizeX; i++)
        {
            for (int j = 0; j < mapSizeY; j++)
            {
                // hexes[i, j] = new Hex(i, j);
            }
        }
        ***************************************************/


    }

    // Update is called once per frame
    void Update()
    {

    }

    /* Tile Selector methods
     * 
     * SetSelector(Vector3) - takes a Vector3 and arranges for the Selector to move to the location
     * HasSelectorArrived(Vector3) - checks if the selector has arrived at its target location
     * GetSelectorPosition() - returns the position of the selector
     * 
     */
    public void SetSelector(Vector3 targetPos)
    {
        // Vector3 startPos = selector.transform.position;
        targetPos += selectorOffset;

        selector.transform.position = Vector3.MoveTowards(selector.transform.position, targetPos, .1f);
    }

    public bool HasSelectorArrived(Vector3 targetPos)
    {
        targetPos += selectorOffset;

        return (Vector3.Distance(selector.transform.position, targetPos) < 0.001f);
    }

    public Vector3 GetSelectorPosition()
    {
        return selector.transform.position;
    }


    /* Player Character interaction methods
     *
     *
     *
     *
     */

    public void SetActiveCharacter(PlayerCharacter nextActiveCharacter)
    {
        activeCharacter = nextActiveCharacter;
    }

    public PlayerCharacter GetActiveCharacter()
    {
        return activeCharacter;
    }

    public void MoveCharacter(PlayerCharacter movingCharacter, Vector3Int targetPos)
    {
        // Debug.Log(movingCharacter.GetStartPos() + ", " + targetPos + ", " + GetTileDistance(movingCharacter.GetStartPos(), targetPos));
        if (GetTileDistance(movingCharacter.GetStartPos(), targetPos) <= movingCharacter.GetStamina()
            && IsWalkableTile(targetPos, movingCharacter))
        {
            ClearMovementOverlay();

            if (!movingCharacter.HasArrived(movingCharacter.GetPosition(), fullMap.CellToWorld(targetPos)))
            {
                movingCharacter.Move(fullMap.CellToWorld(targetPos));
            }

        }

        // Debug.Log(movingCharacter.HasArrived(movingCharacter.GetPosition(), fullMap.CellToWorld(targetPos)));

        if (movingCharacter.HasArrived(movingCharacter.GetPosition(), fullMap.CellToWorld(targetPos)))
        {
            CreateMovementOverlay(movingCharacter.GetStamina(), targetPos);
        }
    }

    public void PathCharacter(PlayerCharacter movingCharacter, Stack<Vector3> path)
    {
        while (path.Count > 0)
        {
            //MoveCharacter(movingCharacter, path.Pop());
        }
    }

    /* Tile Interaction methods
     * 
     * GetTileType(Vector3 pos) => String
     * GetTileDistance(Vector3Int pos1, Vector3Int pos2) => int
     * AxialHexToCube(Vector3 pos) => Vector3Int
     * TravelCost(Vector 3 pos)
     * IsWalkableTile(Vector3 pos)
     * IsAnyoneOnTile(Vector3 pos)
     * OnMap(Vector3 pos)
     * 
     */
    public string GetTileType(Vector3 tilePos)
    {
        int tileOrder = -1;
        string tileType = "";

        // converts Vector3s into Vector3Int since GetTile only takes Vector3Int
        // we allow Vector3 input and sanitize here for easier use
        Vector3Int convertedTilePos = new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z));

        foreach (Tilemap tilemap in hexMaps)
        {
            if (tilemap.GetTile(convertedTilePos))
            {
                if (tileOrder < tilemap.GetComponentInParent<TilemapRenderer>().sortingOrder && tilemap.name != "TileOverlay")
                {
                    tileOrder = tilemap.GetComponentInParent<TilemapRenderer>().sortingOrder;
                    tileType = tilemap.name;
                }

            }
        }

        return tileType;
    }

    public int GetTileDistance(Vector3 hex1, Vector3 hex2)
    {
        Vector3Int hex1Conv = AxialHexToCube(hex1);
        Vector3Int hex2Conv = AxialHexToCube(hex2);

        return (Mathf.Abs(hex1Conv.x - hex2Conv.x) + Mathf.Abs(hex1Conv.y - hex2Conv.y) + Mathf.Abs(hex1Conv.z - hex2Conv.z)) / 2;
    }

    public Vector3Int AxialHexToCube(Vector3 tilePos)
    {
        Vector3Int convertedTilePos = new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z));

        int q = convertedTilePos.x - (convertedTilePos.y - (convertedTilePos.y & 1)) / 2;
        int r = convertedTilePos.y;
        int s = -q - r;
        return (new Vector3Int(q, r, s));
    }

    public int TravelCost(Vector3 tilePos)
    {
        Vector3Int convertedTilePos = new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z));

        // default value is Int32.MaxValue - we would rather players not be able to move to tiles that have no specification
        int travelCost = 2119321354;

        // when determining cost traversals, certain perks would divide the cost of certain tiles by 100;
        foreach (Tilemap tilemap in hexMaps)
        {
            if (tilemap.GetTile(convertedTilePos))
            {
                switch (tilemap.name)
                {
                    case "Water":
                        travelCost = (int)HexType.Water; break;
                    case "Mountains":
                        travelCost = (int)HexType.Mountains; break;
                    case "Trees":
                        travelCost = (int)HexType.Trees; break;
                    case "RaisedLand":
                        travelCost = (int)HexType.RaisedLand; break;
                    case "Land":
                        travelCost = (int)HexType.Land; break;
                }
            }
        }

        // if someone's on the tile, we multiply the cost by 100
        // this way, if someone's on a water tile, the cost for the tile is 10000
        // with a swim perk, the water tile goes down to 100
        // the character would need an extra perk to slip by

        // this needs to be made so allies can be moved through
        if (IsAnyoneOnTile(convertedTilePos))
        {
            travelCost *= 100;
        }

        return travelCost;
    }

    // Needs to be fixed to work based off travel costs
    public bool IsWalkableTile(Vector3 tilePos, PlayerCharacter character)
    {
        Vector3Int convertedTilePos = new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z));
        bool isWalkable = true;

        if (!BFS(character.GetStartPos(), character.GetStamina()).Contains(convertedTilePos))
        {
            isWalkable = false;
        }

        if (IsAnyoneOnTile(convertedTilePos))
        {
            isWalkable = false;
        }

        return isWalkable;
    }

    public bool IsAnyoneOnTile(Vector3 tilePos)
    {
        bool tileHasPerson = false;

        foreach (PlayerCharacter character in mapCharacters)
        {
            if (fullMap.WorldToCell(character.GetPosition()) == tilePos && character != activeCharacter)
            {
                tileHasPerson = true;
            }
        }

        return tileHasPerson;
    }

    // need to create a proper "game entity class" and change this so it takes any kind of object's position
    public bool TileInRange(PlayerCharacter gameObject, Vector3 targetPos, int range)
    {
        return (GetTileDistance(gameObject.GetStartPos(), targetPos) <= range);
    }

    public bool OnMap(Vector3 position)
    {
        return ((position.x < (mapSizeX / 2)) && (position.x > (mapSizeX / -2)) && (position.y > (mapSizeY / -2)) && (position.y < (mapSizeY / 2)));
    }

    /* Movement Overlay creation
     * 
     * DrawMovementOverlay()
     * AddWalkableTile(Vector3 pos)
     * ClearMovementOverlay()
     * CreateMovementOverlay(int stamina, Vector3 centerPos)
     * 
     */

    public List<Vector3> BFS(Vector3 start, int depth)
    {
        Queue frontier = new Queue();
        frontier.Enqueue(start);

        List<Vector3> visited = new List<Vector3>();
        Dictionary<Vector3, int> costSoFar = new Dictionary<Vector3, int>();
        
        visited.Add(start);
        costSoFar.Add(start, 0);
     
        while (frontier.Count != 0) {
            Vector3 current = (Vector3)frontier.Dequeue();
            
            foreach (var next in NeighbourTiles(current))
            {
                if (!visited.Contains(next) && ((TravelCost(next) + costSoFar[current]) <= depth) && OnMap(next))
                { 
                    frontier.Enqueue(next);
                    visited.Add(next);
                    costSoFar.Add(next, costSoFar[current] + TravelCost(next));
                }
            }

        }

        return visited;
    }
    // to use this method, you enter in the character's Stamina and their Position

    public void CreateMovementOverlay(int movementRange, Vector3 tilePos)
    {
        int minY = Mathf.RoundToInt(tilePos.y) - movementRange;
        int maxY = Mathf.RoundToInt(tilePos.y) + movementRange;
        int minX = Mathf.RoundToInt(tilePos.x) - movementRange;
        int maxX = Mathf.RoundToInt(tilePos.x) + movementRange;

        Vector3Int convertedTilePos = AxialHexToCube(tilePos);
        Vector3 testTile;
        Vector3Int axialTestTile;

        foreach (Vector3 pos in BFS(tilePos, movementRange))
        {
            AddWalkableTile(pos);
        }


        DrawMovementOverlay();
        shouldRedraw = false;
    }

    public void DrawMovementOverlay()
    {
        tileOverlay.enabled = true;
        Tile[] walkableTileSprites = new Tile[walkableTileCoords.Count];

        for (int i = 0; i < walkableTileSprites.Length; i++)
        {
            walkableTileSprites[i] = walkingOverlayTile;
        }

        tileOverlay.SetTiles(walkableTileCoords.ToArray(), walkableTileSprites);
    }

    public void AddWalkableTile(Vector3 tilePos)
    {
        walkableTileCoords.Add(new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z)));

    }

    public void ClearMovementOverlay()
    {
        tileOverlay.ClearAllTiles();
        walkableTileCoords.Clear();
    }


    /* A* Search 
     * 
     * The following code deals with pathfinding on the game map.
     * 
     * NeighbourTiles(Vector3) - returns all the neighbouring tiles of a Vector3 location
     * AStarTraversal(Vector3, Vector3, int) finds a path from a start Vector3 to an end Vector.
     * 
     */

    // Directions for the A* to branch out
    public static readonly Vector3[] EVEN_DIRS = new[] {
         new Vector3(1, 0, 0), // to right of tile
         new Vector3(0, -1, 0), // to left of tile
         new Vector3(-1, -1, 0), // below tile
         new Vector3(-1, 0, 0), // above tile
         new Vector3(-1, 1, 0), // diagonal top left
         new Vector3(0, 1, 0) // diagonal bottom left
     };

    public static readonly Vector3[] ODD_DIRS = new[] {
         new Vector3(1, 0, 0), // to right of tile
         new Vector3(1, -1, 0), // to left of tile
         new Vector3(0, -1, 0), // below tile
         new Vector3(-1, 0, 0), // above tile
         new Vector3(0, 1, 0), // diagonal bottom left
         new Vector3(1, 1, 0) // diagonal top left
     };

    public IEnumerable<Vector3> NeighbourTiles(Vector3 start)
    {
        if (Mathf.Abs(start.y % 2) == 0)
        {
            foreach (var dir in EVEN_DIRS)
            {
                Vector3 next = new Vector3(start.x + dir.x, start.y + dir.y, start.z + dir.z);

                yield return next;
            }
        } else if (Mathf.Abs(start.y % 2) == 1)
        {
            foreach (var dir in ODD_DIRS)
            {
                Vector3 next = new Vector3(start.x + dir.x, start.y + dir.y, start.z + dir.z);

                yield return next;
            }
        }
    }

    public Dictionary<Vector3, Vector3> AStarTraversal(Vector3 start, Vector3 destination, int activeCharStam)
    {
        var frontier = new PriorityQueue<Vector3, int>();
        frontier.Enqueue(start, 0);

        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();
        Dictionary<Vector3, int> costSoFar = new Dictionary<Vector3, int>();

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(destination))
            {
                break;
            }

            if (!OnMap(destination))
            {
                break;
            }

            foreach (var next in NeighbourTiles(current))
            {
                int newCost = costSoFar[current] + TravelCost(next);

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + GetTileDistance(next, destination);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        return cameFrom;
    }

    public Stack<Vector3> BuildPath(Vector3 start, Vector3 end, int movementPoints)
    {
        Vector3 node;
        Dictionary<Vector3, Vector3> hexPath = AStarTraversal(start, end, movementPoints);

        Stack<Vector3> path = new Stack<Vector3>();
        path.Push(end);

        foreach (KeyValuePair<Vector3, Vector3> entry in hexPath)
        {
            Debug.Log("Key: " + entry.Key);
            Debug.Log("Entry: " + entry.Value);
        }

        while (hexPath[end] != start)
        {
            if (hexPath.TryGetValue(end, out node))
            {
                path.Push(node);
                end = node;
            }
            else
            {
                Debug.Log("No Path");
                break;
            }
        }

        while (path.Count > 0)
        {
            Debug.Log(path.Pop());
        }

        return path;
    }
}

