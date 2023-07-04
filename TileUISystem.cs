using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TileUISystem : MonoBehaviour
{
    private TextMeshProUGUI tileInfoText;
    private TextMeshProUGUI actionMenuText;

    private string tileTerrainType;

    // Start is called before the first frame update
    void Start()
    {
        tileInfoText = transform.Find("TileInfo").GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        tileInfoText.text = tileTerrainType;

    }

    public void SetTileTerrain(string tileTerrain, Vector3 position)
    {    
        tileTerrainType = tileTerrain + "\n";
        tileTerrainType += position.x + ", " + position.y;

    }
}
