using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Util;

public class MouseInputSystem : MonoBehaviour
{
    private Vector3 mousePos;
    private Vector3Int tilePosition;
    private Vector3Int pathingPlayerPosition;
    private Vector3Int finalPlayerPosition;

    private int moveCost;
    [SerializeField] private Stack<Tuple<Vector3, int>> movementPath;

    private Grid fullMap;
    private Camera cam;

    private HexSystem hexSystem;
    private TileUISystem tileUISystem;
    private PlayerCharacter activeCharacter;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        tileUISystem = GameObject.Find("Canvas").GetComponentInChildren<TileUISystem>();
        hexSystem = transform.GetComponent<HexSystem>();

        activeCharacter = hexSystem.GetActiveCharacter();
        fullMap = transform.GetComponent<Grid>();
        pathingPlayerPosition = activeCharacter.GetCellPosition(fullMap);

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
            finalPlayerPosition = fullMap.WorldToCell(mousePos);
            activeCharacter.SetStartPos(fullMap.WorldToCell(activeCharacter.GetPosition()));
            movementPath = hexSystem.BuildPath(activeCharacter.GetStartPos(), finalPlayerPosition, activeCharacter.GetMovement());            
        }

        // creates the path that the character moves along
        // adjust's character's movement it moves along
        if (movementPath != null && movementPath.Count >= 1)
        {
            pathingPlayerPosition = V3IntConv(movementPath.Peek().Item1);

            if (activeCharacter.HasArrived(activeCharacter.GetPosition(), fullMap.CellToWorld(pathingPlayerPosition)))
            {
                //Debug.Log(movementPath.Peek().Item2);
                activeCharacter.UseMovement(hexSystem.TileTraverseCost(movementPath.Pop().Item1));

                //Debug.Log(movementPath.Count);

                if (movementPath.Count == 0) {
                    Debug.Log(activeCharacter.GetMovement());
                    Debug.Log(activeCharacter.GetCellPosition(fullMap));
                    hexSystem.ClearMovementOverlay();
                    hexSystem.CreateMovementOverlay(activeCharacter.GetMovement(), activeCharacter.GetCellPosition(fullMap));                    
                }
            }
        }

        hexSystem.MoveCharacter(activeCharacter, pathingPlayerPosition);

        if (!hexSystem.HasSelectorArrived(tilePosition))
        {
            hexSystem.SetSelector(fullMap.CellToWorld(tilePosition));
        }

    }

    Tile GetClickedTile()
    {
        return null;
    }
}
