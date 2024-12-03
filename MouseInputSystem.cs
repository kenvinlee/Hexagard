using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Util;

public class MouseInputSystem : MonoBehaviour
{
    private float dragSpeed = 0.2f;

    private Vector3 mousePos;
    private Vector3 dragOrigin;
    private Vector3Int tilePosition;
    private Vector3Int turnStartPosition;
    private Vector3Int pathingPlayerPosition;
    private Vector3Int destinationPosition;

    private int moveCost;
    [SerializeField] private Stack<Tuple<Vector3, int>> movementPath;

    private Grid fullMap;
    private Camera cam;

    private CameraSystem camSystem;
    private HexSystem hexSystem;
    private TileUISystem tileUISystem;
    private Character activeCharacter;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        camSystem = cam.GetComponent<CameraSystem>();
        tileUISystem = GameObject.Find("OverlayElements").GetComponentInChildren<TileUISystem>();
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

        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // forward
        {
            camSystem.ZoomIn();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f) // backwards
        {
            camSystem.ZoomOut();
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Instead of moving instantly, this should check the active character, get its starting position,
            // get the destination position, then start building the path 
            
            activeCharacter = hexSystem.GetActiveCharacter();
            turnStartPosition = activeCharacter.GetCellPosition(fullMap);
            pathingPlayerPosition = activeCharacter.GetCellPosition(fullMap);
            
            destinationPosition = fullMap.WorldToCell(mousePos);

            activeCharacter.SetStartPos(fullMap.WorldToCell(activeCharacter.GetPosition()));
            movementPath = hexSystem.BuildPath(activeCharacter.GetStartPos(), destinationPosition, activeCharacter.GetMovement());   
            hexSystem.ClearAttackOverlay();
        }

        // creates the path that the character moves along
        // adjust's character's movement it moves along
        if (movementPath != null && movementPath.Count >= 1)
        {
            pathingPlayerPosition = V3IntConv(movementPath.Peek().Item1);

            if (activeCharacter.HasArrived(activeCharacter.GetPosition(), fullMap.CellToWorld(pathingPlayerPosition)))
            {
                // The traverse cost isn't computing correctly here, need to figure this out
                Debug.Log(hexSystem.ComputeTravelCost(activeCharacter.GetPosition(), fullMap.CellToWorld(pathingPlayerPosition)));
                activeCharacter.UseMovement(hexSystem.TileTraverseCost(movementPath.Pop().Item1));

                //Debug.Log(movementPath.Count);

                if (movementPath.Count == 0) {
                    // Debug.Log(activeCharacter.GetMovement());
                    // Debug.Log(activeCharacter.GetCellPosition(fullMap));
                    hexSystem.AddAttackableTile(activeCharacter.GetCellPosition(fullMap));
                    hexSystem.ClearMovementOverlay();
                    hexSystem.CreateMovementOverlay(activeCharacter.GetMovement(), activeCharacter.GetCellPosition(fullMap));                    
                }
            }
        }

        // now we move the character
        hexSystem.MoveCharacter(activeCharacter, pathingPlayerPosition);

        // move the tile selector
        if (!hexSystem.HasSelectorArrived(tilePosition))
        {
            hexSystem.SetSelector(fullMap.CellToWorld(tilePosition));
        }

        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(2)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);

        cam.GetComponent<Transform>().Translate(move, Space.World);
    }

    public void CheckActiveCharacter()
    {
        activeCharacter = hexSystem.GetActiveCharacter();
        pathingPlayerPosition = activeCharacter.GetCellPosition(fullMap);
    }

    public Vector3 GetClickedTile()
    {
        return tilePosition;
    }
}
