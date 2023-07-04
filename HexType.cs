using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HexType
{ 
    public string HexTypeName;
    public string Obstacle = "";
    // if MovementCost == 0, tile cannot be traversed.
    public int MovementCost = 1;

}
