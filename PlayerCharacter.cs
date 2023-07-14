using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    // basics character info
    private string characterName;
    private string tier1Class;
    private string tier2Class;
    private string tier3Class;

    private string[] perks;
    private bool isAlly;

    // primary stats
    private int health;
    private int strength;
    private int agility;
    private int intellect;
    private int charisma;
    private int luck;

    // secondary stats
    [SerializeField] private int minDamage, maxDamage;
    [SerializeField] private int penetration;
    [SerializeField] private float critChance;
    [SerializeField] private int initiative;

    // tertiary stats
    [SerializeField] private int armour;
    [SerializeField] private int resistance;
    [SerializeField] private int stamina;
    [SerializeField] private float evasion;

    // combat stuff, possibly change the data type
    private string[] buffs;
    private string[] debuffs;
    private int range;

    private int actionPoints;
    private int movementPoints;

    // equipment

    // inventory

    // spells

    // render stuff
    private Vector3 offset = new Vector3(0, 0.2f, 0);
    private Vector3Int moveStartPos;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 targetPos)
    {
        targetPos += offset;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, .03f);
    }

    public bool HasArrived(Vector3 currPos, Vector3 targetPos)
    {
        targetPos += offset;
        
        // Debug.Log(currPos + ", " + targetPos); 
        return (Vector3.Distance(currPos, targetPos) < 0.000001f);
    }

    public void Attack()
    {

    }

    public void SetStamina(int newStam)
    {
        stamina = newStam;
    }

    public int GetStamina()
    {
        return stamina;
    }

    public void ResetMovement()
    {
        movementPoints = stamina;
    }

    public void UseMovement(int cost)
    {
        movementPoints -= cost;
    }

    public int GetMovement()
    {
        return movementPoints;
    }

    public void setAllegiance(bool isAlly)
    {
        this.isAlly = isAlly;
    }

    public bool isEnemy()
    {
        return !isAlly;
    }

    public void SetStartPos(Vector3Int startPos)
    {
        moveStartPos = startPos;
    }

    public Vector3Int GetStartPos()
    {
        return moveStartPos;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3Int GetCellPosition(Grid grid)
    {
        return grid.WorldToCell(transform.position);
    }
}
