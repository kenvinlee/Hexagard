using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Character : MonoBehaviour
{
    // basics character info
    private string characterName;
    private string tier1Class;
    private string tier2Class;
    private string tier3Class;

    private string[] perks;
    private bool isAlly;

    // primary stats
    private int currentHP;
    [SerializeField] private int maxHP;
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
    [SerializeField] private int movementPoints;

    // equipment

    // inventory

    // spells

    // render stuff
    private Transform characterModel;
    private Animator cAnimator;
    private SpriteRenderer cRenderer;
    private TextMeshProUGUI healthText;
    private Slider healthSlider;


    private Vector3 offset = new Vector3(0, 0.2f, 0);
    private Vector3Int moveStartPos;

    // Start is called before the first frame update
    void Start()
    {
        characterName = transform.name;
        
        healthSlider = transform.GetComponentInChildren<Slider>();

        SetCurrentHP(maxHP);
        healthSlider.maxValue = maxHP;

        cAnimator = gameObject.GetComponent<Animator>();
        cRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SetCurrentHP(currentHP - 1);
        }

        healthSlider.value = GetCurrentHP();
    }

    public void Move(Vector3 targetPos)
    {
        targetPos += offset;

        if (targetPos.x < GetStartPos().x)
        {
            cRenderer.flipX = true;
        } 
        else 
        {
            cRenderer.flipX = false;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, .03f);
        cAnimator.SetTrigger("Walking");
    }

    // This is comparing World locations, not Cell locations
    // A character can be in the same Cell but in different world locations, so comparing world locations is more accurate
    public bool HasArrived(Vector3 currPos, Vector3 targetPos)
    {
        targetPos += offset;
        cAnimator.ResetTrigger("Walking");

        return (Vector3.Distance(currPos, targetPos) < 0.000001f);
    }

    public bool IsOutOfActions()
    {
        // && actionPoints <= 0
        if (movementPoints <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void MeleeAttack(IEnumerable<Vector3> neighbours)
    {
        
    }

    public void RangedAttack()
    {

    }

    public string GetName()
    {
        return characterName;
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }

    public void SetCurrentHP(int newHP)
    {
        currentHP = newHP;
    }

    public int GetMaxHP() 
    {
        return maxHP;
    }

    public void SetMaxHP(int newMaxHP)
    {
        maxHP = newMaxHP;
    }

    public void DeathCheckAndHandle()
    {
        if (currentHP > 0)
        {

        }
    }

    public int GetInitiative()
    {
        return initiative;
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
