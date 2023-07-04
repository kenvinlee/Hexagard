using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HexSystem : MonoBehaviour
{

    // selector
    public GameObject selector;
    private Vector3 selectorOffset = new Vector3(0, -0.13f, 0);

    // character
    public PlayerCharacter testCharacter;
    public PlayerCharacter activeCharacter;

    // Grids and Tilemaps
    private Grid fullMap;
    private Tilemap tileOverlay; 
    private Tilemap[] hexMaps;
    private TilemapRenderer[] hexMapRenderers;
    private HexType[] hexTypes;

    private int mapXCoord, mapYCoord;
    private int mapSizeX, mapSizeY;
    [SerializeField] private List<Vector3Int> walkableTileCoords;

    [SerializeField] private Tile walkingOverlayTile;

    // possibly useless
    private Hex[,] hexes;
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
        testCharacter = GameObject.Find("Player").GetComponent<PlayerCharacter>();

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

        hexes = new Hex[mapSizeX, mapSizeY];
        hexTypes = new HexType[hexMaps.Length];

        // Create hex array based on above
        for (int i = 0; i < hexTypes.Length; i++)
        {
            hexTypes[i] = new HexType();
            hexTypes[i].HexTypeName = hexMaps[i].name;

        }

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

        testCharacter.SetStamina(3);
        CreateMovementOverlay(testCharacter.GetStamina(), testCharacter.GetPosition());

        /*
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
        }*/
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


    /* Player Character methods
     *
     *
     *
     *
     */
    
    public void SetActiveCharacter(PlayerCharacter nextActiveCharacter)
    {
        activeCharacter = nextActiveCharacter;
    }

    public void MoveCharacter(PlayerCharacter movingCharacter, Vector3Int targetPos)
    {
        // Debug.Log(movingCharacter.GetStartPos() + ", " + targetPos + ", " + GetTileDistance(movingCharacter.GetStartPos(), targetPos));

        if (GetTileDistance(AxialHexToCube(movingCharacter.GetStartPos()), AxialHexToCube(targetPos)) <= movingCharacter.GetStamina()
            && IsWalkableTile(targetPos))
        {
            ClearMovementOverlay();

            if (!movingCharacter.HasArrived(movingCharacter.GetPosition(), fullMap.CellToWorld(targetPos)))
            {
                movingCharacter.Move(fullMap.CellToWorld(targetPos));
            }

         }

        Debug.Log(movingCharacter.HasArrived(movingCharacter.GetPosition(), fullMap.CellToWorld(targetPos)));

        if (movingCharacter.HasArrived(movingCharacter.GetPosition(), fullMap.CellToWorld(targetPos)))
        {
            CreateMovementOverlay(movingCharacter.GetStamina(), targetPos);
        }
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

        for (int i = minY; i <= maxY; i++)
        {
            for (int j = minX; j <= maxX; j++)
            {
                testTile = new Vector3(j, i, 0);
                axialTestTile = AxialHexToCube(testTile);

                if (GetTileType(testTile) != "" && 
                    IsWalkableTile(testTile) && 
                    GetTileDistance(convertedTilePos, axialTestTile) <= movementRange)
                {
                    AddWalkableTile(testTile);
                    
                }
            }
        }

        DrawMovementOverlay();
        shouldRedraw = false;
    }

    /* Tile Interaction methods
     * 
     * 
     * 
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

    public int GetTileDistance(Vector3Int hex1, Vector3Int hex2)
    {
        return (Mathf.Abs(hex1.x - hex2.x) + Mathf.Abs(hex1.y - hex2.y) + Mathf.Abs(hex1.z - hex2.z)) / 2;
    }

    public Vector3Int AxialHexToCube(Vector3 tilePos)
    {
        Vector3Int convertedTilePos = new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z));

        int q = convertedTilePos.x - (convertedTilePos.y - (convertedTilePos.y & 1)) / 2; 
        int r = convertedTilePos.y;
        int s = -q - r;
        return (new Vector3Int(q, r, s));
    }

    public bool IsWalkableTile(Vector3 tilePos)
    {
        Vector3Int convertedTilePos = new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z));
        bool isWalkable = true;

        foreach (Tilemap tilemap in hexMaps)
        {
            if (tilemap.GetTile(convertedTilePos))
            {
                switch (tilemap.name)
                {
                    case "Water":
                        isWalkable = false; break;
                    case "Mountains":
                        isWalkable = false; break;
                    case "Trees:":
                        isWalkable = false; break;
                    default:
                        isWalkable = true; break;
                }
            }
        }

        return isWalkable;
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

    // need to create a proper "game entity class" and change this so it takes any kind of object's position
    public bool TileInRange(PlayerCharacter gameObject, Vector3 targetPos, int range)
    {
        return (GetTileDistance(AxialHexToCube(gameObject.GetStartPos()), AxialHexToCube(targetPos)) <= range);
    }

    public void ClearMovementOverlay()
    {
        tileOverlay.ClearAllTiles();
        walkableTileCoords.Clear();
    }
}
