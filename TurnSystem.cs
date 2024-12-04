using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;


public class TurnSystem : MonoBehaviour
{
    private bool isNewTurn;
    private List<Character> characters;
    private List<Character> orderedCharacters;
    private List<Character> reinforcements;
    private PriorityQueue<Character, int> initiativeQueue = new PriorityQueue<Character, int>();


    private TurnUISystem turnUISystem;

    void Awake()
    {
        orderedCharacters = new List<Character>();
    }

    // Start is called before the first frame update
    void Start()
    {
        characters = new List<Character>(transform.GetComponentsInChildren<Character>());
        turnUISystem = GameObject.Find("OverlayElements").GetComponentInChildren<TurnUISystem>();
        turnUISystem.SetTurnOrder(orderedCharacters);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reinforcements(List<Character> reinforcements)
    {
        foreach (Character reinforcement in reinforcements)
        {
            initiativeQueue.Enqueue(reinforcement, reinforcement.GetInitiative());
        }

        orderedCharacters.Clear();

        while (initiativeQueue.Count > 0)
        {
            orderedCharacters.Add(initiativeQueue.Dequeue());
        }

        orderedCharacters.Reverse();

        foreach (Character character in orderedCharacters)
        {
            initiativeQueue.Enqueue(character, character.GetInitiative());
        }

    }

    public void NewTurn(List<Character> characters)
    {
        foreach (Character character in characters)
        {
            initiativeQueue.Enqueue(character, character.GetInitiative());
        }

        orderedCharacters.Clear();

        while (initiativeQueue.Count > 0)
        {
            orderedCharacters.Add(initiativeQueue.Dequeue());
        }

        orderedCharacters.Reverse();

        foreach (Character character in orderedCharacters)
        {
            initiativeQueue.Enqueue(character, character.GetInitiative());
        }

    }

    public Character NextActive()
    {
        Character nextCharacter;

        if (initiativeQueue.Count > 0)
        {
            nextCharacter = initiativeQueue.Dequeue();
            return nextCharacter;
        }
        else
        {
            Debug.Log("new turn");
            isNewTurn = true;
            NewTurn(characters);
            turnUISystem.SetTurnOrder(orderedCharacters);
            return initiativeQueue.Dequeue();
        }
    }

    public bool IsNewTurn()
    {
        if (isNewTurn)
        {
            isNewTurn = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateUI()
    {
        if (orderedCharacters.Count > 0) { 
            orderedCharacters.RemoveAt(orderedCharacters.Count - 1);
            turnUISystem.SetTurnOrder(orderedCharacters);
        }
    }
}
