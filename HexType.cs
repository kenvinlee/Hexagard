using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum HexType
{
    /* number explanation:
     * 
     **** If the numbers are less than 10 ****
     * 
     * The number represent how much stamina it costs 
     * to move into the specific tile type
     * 
     **** If the numbers are greater than 10 ****
     * There should be a spell or passive that reduces the 
     * cost to traverse. For example, the "Swimming" Perk
     * would cause the Water tile cost to be divided by 100.
     *  
    */

    Water = 100,
    RaisedLand = 2,
    Land = 1,
    Mountains = 200,
    Trees = 2,
}
