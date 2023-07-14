using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MouseInputSystem : MonoBehaviour
{
    private Vector3 mousePos;
    private Vector3Int tilePosition;
    private Vector3Int newPlayerPosition;

    private Grid fullMap;
    private Camera cam;

    private HexSystem hexSystem;
    private TileUISystem tileUISystem;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        tileUISystem = GameObject.Find("Canvas").GetComponentInChildren<TileUISystem>();
        hexSystem = transform.GetComponent<HexSystem>();


        fullMap = transform.GetComponent<Grid>();

        mousePos = Input.mousePosition;
        
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        tilePosition = fullMap.WorldToCell(mousePos);
        tileUISystem.SetTileTerrain(hexSystem.GetTileType(tilePosition), tilePosition);

        if (Input.GetMouseButtonUp(0))
        {
            newPlayerPosition = fullMap.WorldToCell(mousePos);
            hexSystem.GetActiveCharacter().SetStartPos(fullMap.WorldToCell(hexSystem.GetActiveCharacter().GetPosition()));

        }

        if (!hexSystem.HasSelectorArrived(tilePosition))
        {
            hexSystem.SetSelector(fullMap.CellToWorld(tilePosition));
        }

        hexSystem.MoveCharacter(hexSystem.GetActiveCharacter(), newPlayerPosition);

        Debug.Log(hexSystem.OnMap(tilePosition));
        // Debug.Log(hexSystem.GetSelectorPosition());
    }

    Tile GetClickedTile()
    {
        return null;
    }
}
