
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;


//TODO: should be renamed to character
public abstract class Character : MonoBehaviour
{
    public enum CharacterState
    {
        Idling, Attacking, Travelling, Fleeing, Dead
    }

    public CharacterState State;


    [HideInInspector] public TeamController Team;

    [Header("Movement")]
    public float RunSpeed;
    public bool Walking;
    public float WalkSpeed;
    public float AttackRange;

    [HideInInspector]
    //should ignore z for 2d.
    public Vector3 Target;

    public class TargetDeathEvent : UnityEvent{ }
    public TargetDeathEvent OnTargetDeath = new TargetDeathEvent();

    //TODO: test that it works using the same event class
    public class AttackEvent : UnityEvent<Character> { }
    public AttackEvent OnAttackCharacter = new AttackEvent();
    public AttackEvent OnBeingAttacked = new AttackEvent();


    private Character _attackTarget;
    public Character AttackTarget
    {
        get { return _attackTarget; }
        set
        {
            if (_attackTarget == value) return;
            _attackTarget = value;
            if(!value) return;
            OnAttackCharacter.Invoke(_attackTarget);
            _attackTarget.OnDeath.AddListener(x=>OnTargetDeath.Invoke());

        }
    }

    //Should they have a seperate move target? so they can remember it.

    [Header("Stats")]
    [SerializeField]
    private int _health = 10;
    public int Health
    {
        get { return _health; }
        set
        {
            if (value == _health) return;
            if (value <= 0)
                OnDeath.Invoke(this);
            if (value < _health)
                OnDamage.Invoke(_health - value);
            _health = value;
        }
    }

    //TODO: should probably be an interval with min/max
    public int Damage = 1;

    //TODO: all typical values should have a modifier List with val, modType and modName, which makees it easier to add and remove modifiers from equipment and stuff. 
    //TODO: also values can be easily shown as a total or with all modifier displayed
    public float AttackTime;

    [Header("Moral stats")]

    private int _moral = 10;

    //TODO: use a max moral and have a current of the stat
    public int Moral
    {
        get { return _moral; }
        set
        {
            if (value == _moral) return;
            _moral = value;
            Debug.Log(gameObject.name + " lost " + value + " moral");
            if (_moral <= 0) State = CharacterState.Fleeing;
        }
    }

    //maybe hide all these in inspector:

    public float MoralLossOnDmgMod = 1f;
    public int MoralLossOnFriendDeath = 5;
    public int MoralBoostOnEnemyDeath = 5;
    public int MoralBoostOnKill = 5;

    [Header("Sprite")]
    //public SpriteRenderer CharacterSprite;
    public Color DamageColor, NormalColor;
    
    public class DamageEvent : UnityEvent<int> { }
    public DamageEvent OnDamage = new DamageEvent();

    //using self as parameter, so other listeners will know who dead
    public class DeathEvent : UnityEvent<Character> { }
    public  DeathEvent OnDeath = new DeathEvent();

    private Collider2D collider;
    private Coroutine _attackRoutine;

    public void Start()
    {
        //CharacterSprite = GetComponent<SpriteRenderer>();
        //NormalColor = CharacterSprite.color;
        DamageColor = Color.red;
        
        OnDamage.AddListener(x=> StartCoroutine(HurtRoutine()));
        OnDeath.AddListener(Die);
        
        AttackRange = transform.lossyScale.x * 2f;

        OnTargetDeath.AddListener(TargetGone);
        OnBeingAttacked.AddListener(BeingAttacked);
        OnAttackCharacter.AddListener(AttackCharacter);
    }

    void FixedUpdate()
    {
        //TODO: merge together with move's switch statement
        if (AttackTarget && AttackTarget.isActiveAndEnabled && InAttackRange()) //has live enemy target and in attackrange
        {
            if(_attackRoutine == null)
                _attackRoutine = StartCoroutine(AttackRoutine());
        }
        else
            Move();
    }

    #region Private methods

    protected bool Attacking()
    {
        return State == CharacterState.Attacking;
    }
    bool Fleeing()
    {
        return State == CharacterState.Fleeing;
    }
    bool Idling()
    {
        return State == CharacterState.Idling;
    }


    private Character GetClosestEnemy()
    {
        var playerChar = (gameObject.tag == "Player");
        var enemyTag = playerChar ? "NPC" : "Player";

        //get these from a game or fight controller instead for maintenance
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        if (!closest) return null;
        return closest.GetComponent<Character>();
    }

    private void TargetGone()
    {
        if(!Attacking()) return;

        var closest = GetClosestEnemy();
        if (!closest)
        {
            State = CharacterState.Idling;
            AttackTarget = null;
        }
        else
            AttackTarget = closest;
    }

    //Should only be called through setting the attack target
    private void AttackCharacter(Character target)
    {
        target.OnBeingAttacked.Invoke(this);
    }

    private void BeingAttacked(Character attacker)
    {
        if(Fleeing()) return;

        if (!Attacking() || AttackTarget == null)
        {
            State = CharacterState.Attacking;
            var closest = GetClosestEnemy();
            
            AttackTarget = closest ? closest : attacker;
        }
    }


    private IEnumerator AttackRoutine()
    {
        Debug.Log(gameObject.name + " is Attacking " + AttackTarget.gameObject.name);
        
        State = CharacterState.Attacking;
        while (Attacking() && InAttackRange())
        {
            //HIT TARGET
            AttackTarget.Health -= Damage;

            Debug.Log(gameObject.name + " hit " + AttackTarget.gameObject.name +" for " + Damage + " damage");

            //should be tied to animation maybe?
            yield return new WaitForSeconds(AttackTime);
        }

        _attackRoutine = null;
    }

    private  void Move()
    {
        Vector3 moveDirection;

        switch (State)
        {
            case CharacterState.Idling:
                //TODO: something random
                break;
            case CharacterState.Attacking:
                if (AttackTarget)
                {
                    Target = AttackTarget.transform.position;

                    moveDirection = (Target - transform.position).normalized + new Vector3(Random.value-0.5f,Random.value-0.5f);
                    
                    transform.position += moveDirection * (Walking ? WalkSpeed : RunSpeed);
                }
                else
                {
                    TargetGone();
                }
                break;
            case CharacterState.Travelling:
                //check for arrival and stop travelling
                if (Vector2.Distance(transform.position,Target) < 1)
                {
                    Debug.Log(name +" arrived at target");
                    State = CharacterState.Idling;
                }

                moveDirection = (Target - transform.position).normalized;
                

                transform.position += moveDirection * (Walking ? WalkSpeed : RunSpeed);
                break;
            case CharacterState.Fleeing:
                if (AttackTarget)
                {
                    Target = AttackTarget.transform.position;

                    moveDirection = (Target - transform.position).normalized;

                    Walking = false;
                    moveDirection *= -1;

                    transform.position += moveDirection * (Walking ? WalkSpeed : RunSpeed);
                }
                else
                    State = CharacterState.Idling;
                break;
            case CharacterState.Dead:
                break;
            default:
                break;
        }

    }

    //could take a damage parameter
    public IEnumerator HurtRoutine()
    {
        //CharacterSprite.color = DamageColor;

        yield return new WaitForSeconds(0.1f);

        //CharacterSprite.color = NormalColor;
    }


    private void Die(Character self)
    {
        //TODO: remove this at some point
        gameObject.SetActive(false);

        State = CharacterState.Dead;
    }

    private bool InAttackRange()
    {
        if (!AttackTarget)
            return false;

        var targetCol = AttackTarget.GetComponent<BoxCollider2D>();

        if (!targetCol)
            return false;

        var boxCol = GetComponent<BoxCollider2D>();

        return (boxCol.Distance(targetCol).distance <= AttackRange);

    }

    #endregion
}
