
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;


//TODO: should be renamed to character
public abstract class Character : MonoBehaviour
{
    //Should we use different state for travelling and just looking at something clsoe by
    public enum CharacterState
    {
        Idling, Attacking, Travelling, Fleeing, Hiding, Dead
    }

    public CharacterState State;
    

    public struct Stat
    {
        //public enum ModType
        //{
        //    Multiplier, Addition
        //}

        public struct StatMod
        {
            public string Name;
            //public ModType Type;
            public float Modifier;

            public StatMod(string name,float modifier)
            {
                Name = name;
                //Type = type;
                Modifier = modifier;
            }
        }

        public string Name;
        private int Max;
        //TODO: should decreases like damage be done with modifiers instead?
        
        public List<StatMod> Modifiers;

        public Stat(string name, int max) 
        {
            Name = name;
            Max = max;
            Modifiers = new List<StatMod>();
        }

        //accumulation first and then each multiplier is done at the base
        //public  int GetStatValue()
        //{
        //    var x = CurrentValue;
        //    foreach (var mod in Modifiers)
        //    {
        //        x += mod.Modifier;
        //    }
        //    return x % 1 < 0.5f ? (int)x : (int)x + 1;
        //}
        public int GetStatMax()
        {
            float x = Max;

            foreach (var mod in Modifiers)
            {
                x += mod.Modifier;
            }

            return x % 1 < 0.5f ? (int)x : (int)x + 1;
        }

        //accumulation first and then each multiplier is done at the base
        public string GetStatDescription()
        {
            string x = Name + ": " + Max + "(base)";

            foreach (var mod in Modifiers)
            {
                x += " + " + mod.Modifier.ToString("F") + "(" + mod.Name + ")";
            }

            x += " = " + GetStatMax();

            return x;
        }
    }


    [HideInInspector] public TeamController Team;

    [Header("Stats")]
    //Damage All(from 1 to DAM)
    //Aim
    //Attention scout, diplomat, Ambusher
    //courage/guts/daring/bravery Swarmer
    //Health Swarmer
    //Speed Ambusher, Scout
    //Smarts? Slave, Diplomat, Shaman, Necromancer

    //TODO: use a max for stats;
    public Stat DMG;
    public Stat AIM;
    public Stat ATT;
    public Stat COU;
    public Stat HEA;
    public Stat SPE;
    public Stat SMA ;
    [SerializeField]
    public List<Stat> Stats;

    public int DamMin,
        DamMax,
        AimMin,
        AimMax,
        AttMin,
        AttMax,
        CouMin,
        CouMax,
        HeaMin,
        HeaMax,
        SpeMin,
        SpeMax,
        SmaMin,
        SmaMax;
    //TODO: should probably be an interval with min/max
    //public int Damage = 1;
    //public int Aim = 5;
    //public int Intelligence = 5;


    [Header("Movement")]
    public bool Walking;
    public float AttackRange;
    
    private bool idleAction;

    [HideInInspector]
    //should ignore z for 2d.
    //public Vector3 Target;

    public class TargetDeathEvent : UnityEvent{ }
    public TargetDeathEvent OnTargetDeath = new TargetDeathEvent();

    //TODO: test that it works using the same event class
    public class AttackEvent : UnityEvent<Character> { }
    public AttackEvent OnAttackCharacter = new AttackEvent();
    public AttackEvent OnBeingAttacked = new AttackEvent();

    public Vector3 Target;
    public NavMeshAgent navMeshAgent;

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
    private Hidable hiding;

    public Hidable Hidingplace
    {
        get { return hiding; }
        set
        {
            if (hiding)
                hiding.OccupiedBy = null;
            if(!value) return;

            value.OccupiedBy = this;
            hiding = value;
        }
    }


    [Header("Levelling")]
    public static int[] LevelCaps = {0,10,20,30,50,80,130,210,340, 550,890, 20000};
    
    public class LevelUp : UnityEvent { }
    public LevelUp OnLevelUp = new LevelUp();

    private static int GetLevel(int xp)
    {
        int level = 0;
        
        while(LevelCaps[level++] < xp) { }

        return level;
    }

    public int CurrentLevel = GetLevel(0);
    private float xp = 0;

    public float Xp
    {
        get
        {
            return xp;
        }
        set
        {
            if(value == xp)
                return;
            xp = value;
            var lvl =GetLevel((int)value);
            //TODO: should check for extra level 
            if (lvl > CurrentLevel)
            {
                CurrentLevel++;
                OnLevelUp.Invoke();
            }
        }
    }

    public void Start()
    {
        //------------------------- STAT SET-UP --------------------------
        DMG = new Stat("DAMAGE", Random.Range(DamMin, DamMax));
        AIM = new Stat("AIM", Random.Range(AimMin, AimMax));
        ATT = new Stat("ATTENTION", Random.Range(1, 4));
        COU = new Stat("COURAGE", Random.Range(3, 10));
        HEA = new Stat("HEALTH", Random.Range(5, 15));
        SPE = new Stat("SPEED", Random.Range(3, 5));
        SMA = new Stat("SMARTS", Random.Range(3, 6));
        Stats = new List<Stat>() {DMG,AIM,ATT,COU,HEA,SPE,SMA};

        Health = HEA.GetStatMax();
        
        //CharacterSprite = GetComponent<SpriteRenderer>();
        //NormalColor = CharacterSprite.color;
        DamageColor = Color.red;
        
        OnDamage.AddListener(x=> StartCoroutine(HurtRoutine()));
        OnDeath.AddListener(Die);
        
        AttackRange = transform.lossyScale.x * 2f;

        OnTargetDeath.AddListener(TargetGone);
        OnBeingAttacked.AddListener(BeingAttacked);
        OnAttackCharacter.AddListener(AttackCharacter);

        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.speed = SPE.GetStatMax();

        if(!navMeshAgent) Debug.LogWarning(name+ ": character does not have Nav Mesh Agent");
    }

    void FixedUpdate()
    {
        navMeshAgent.speed = SPE.GetStatMax();

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

    private void NextLevel()
    {
        //Set some level icon active

        //a sound
    }

    //protected void UpdateStatValues()
    //{
    //    //Only goblins change their values
    //    //maybe use current value for health...
    //    var gob = this as Goblin;
    //    if (gob)
    //        gob.Awareness = ATT.GetStatValue();
    //    Moral = COU.GetStatMax();
    //    Health = HEA.GetStatValue();
    //    navMeshAgent.speed = SPE.GetStatMax();
    //    //SMARTS unused
    //}

    protected bool Travelling()
    {
        return State == CharacterState.Travelling;
    }

    protected bool Attacking()
    {
        return State == CharacterState.Attacking;
    }
    protected bool Fleeing()
    {
        return State == CharacterState.Fleeing;
    }
    protected bool Idling()
    {
        return State == CharacterState.Idling;
    }


    private Hidable GetClosestHidingPlace()
    {

        //get these from a game or fight controller instead for maintenance
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Hidable");
        Hidable closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (Hidable go in gos.Select(h=>h.GetComponent<Hidable>()))
        {
            if(!go)
                Debug.LogError("Hidable object does not have hidable script");
            if(go.OccupiedBy != null)
                continue;
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        if (!closest) return null;
        return closest;
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
        while (Attacking() && InAttackRange() && AttackTarget)
        {
            //HIT TARGET
            var Damage = Random.Range(1, DMG.GetStatMax());
            AttackTarget.Health -= Damage;

            Debug.Log(gameObject.name + " hit " + AttackTarget.gameObject.name +" for " + Damage + " damage");

            //should be tied to animation maybe?
            yield return new WaitForSeconds(AttackTime);
        }

        _attackRoutine = null;
    }

    private  void Move()
    {

        switch (State)
        {
            case CharacterState.Idling:
                if (idleAction)
                {
                    if(navMeshAgent.remainingDistance < 0.02f)
                        idleAction = false;
                    
                }
                else if (Random.value < 0.015f) //selecting idle action
                {
                    idleAction = true;
                    var idleDistance = 4;

                    navMeshAgent.SetDestination(transform.position + new Vector3(Random.Range(-idleDistance, idleDistance), 0,
                                 Random.Range(-idleDistance, idleDistance)));

                    Walking = Random.value < 0.75f;
                }

                break;
            case CharacterState.Attacking:
                if (AttackTarget)
                {
                    navMeshAgent.SetDestination(AttackTarget.transform.position);

                    //TODO: add random factor
                }
                else
                {
                    TargetGone();
                }
                break;
            case CharacterState.Travelling:
                //check for arrival and stop travelling
                if (Vector3.Distance(transform.position, Target) < 1f)
                {
                    //Debug.Log(name +" arrived at target");
                    State = CharacterState.Idling;
                    break;
                }
                
                break;
            case CharacterState.Fleeing:
                if (AttackTarget)
                {
                    //TODO: choose a better flee destination and check once there
                    navMeshAgent.SetDestination(AttackTarget.transform.position * -1);

                    Walking = false;
                }
                else
                    State = CharacterState.Idling;
                break;
            case CharacterState.Dead:
                break;
            case CharacterState.Hiding:
                if (!Hidingplace)
                {
                    State = CharacterState.Idling;
                }

                navMeshAgent.SetDestination( hiding.HideLocation.transform.position);
                
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

        //TODO: remove listeners
        OnDamage.RemoveAllListeners();

        State = CharacterState.Dead;
    }

    private bool InAttackRange()
    {


        if (!AttackTarget)
            return false;

        var targetCol = AttackTarget.GetComponent<CapsuleCollider>();

        if (!targetCol)
        {
            Debug.LogWarning(name+ "'s target does not have a capsule collider");
            return false;
        }

        var boxCol = GetComponent<CapsuleCollider>();

        return ((boxCol.transform.position -(targetCol.transform.position) ).magnitude 
            <= boxCol.radius*boxCol.transform.lossyScale.x
            + targetCol.radius*targetCol.transform.lossyScale.x+AttackRange);

    }

    #endregion

    public void Hide()
    {
        Hidingplace = GetClosestHidingPlace();

        if (!Hidingplace) return;
        
        State = CharacterState.Hiding;
    }
}
