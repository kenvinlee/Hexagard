using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static Vector3Int V3IntConv(Vector3 tilePos) {
        return (new Vector3Int(Mathf.RoundToInt(tilePos.x), Mathf.RoundToInt(tilePos.y), Mathf.RoundToInt(tilePos.z)));
    }

}
