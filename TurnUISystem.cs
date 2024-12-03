using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurnUISystem : MonoBehaviour
{
    // Start is called before the first frame update
    private TextMeshProUGUI turnInfoText;
    private TextMeshProUGUI actionMenuText;
    private List<TextMeshProUGUI> characterPortraits = new List<TextMeshProUGUI>();

    private List<Character> turnCharacters = new List<Character>();

    void Start()
    {
        turnInfoText = transform.Find("TurnOrder").GetComponent<TextMeshProUGUI>();
        turnCharacters = new List<Character>();
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    // Characters will be stored in reverse, so we iterate in reverse
    public void SetTurnOrder(List<Character> characters)
    {
        turnInfoText.text = "";
        turnCharacters = characters;

        for (int i = characters.Count - 1; i >= 0; i--)
        {
            turnInfoText.text += characters[i].GetName() + "\n";
        }
    }

}
