
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;


//TODO: Should be divided into smaller classes
public abstract class Character : MonoBehaviour
{
    [Header("Debug Values")]
    public float AgentVelocity;
    public float DesiredVelocity;
    public bool HasPath;
    public bool PathStale;
    public bool IsOnNavMesh;


    //Should we use different state for travelling and just looking at something clsoe by
    public enum CharacterState
    {
        Idling, Attacking, Travelling, Fleeing, Hiding, Dead,Watching,Searching,Provoking, Surprised, Resting
    }

    public enum Race
    {
        Goblin, Human, Spider, Zombie, Ogre, Wolf,
        NoRace
    }

    public GameObject HolderGameObject;

    public Race CharacterRace;

    [Header("Enemy specific")]
    public bool StickToRoad;
    public bool HasEquipment;

    [Header("Voice")]
    public AudioSource Voice;

    public static readonly float PitchMin = 1.3f;
    public static readonly float PitchMax = 2.5f;
    public float VoicePitch;
    public TextMeshProUGUI VoiceText;
    public float ShoutTime = 2f;
    public AudioSource MovementAudio;

    public float SurprisedTime = 4;
    public float SurprisedStartTime;

    public CharacterState State;
    

    public class Stat
    {

        [Serializable]
        public struct StatMod
        {
            public string Name;
            //public ModType Type;
            public int Modifier;

            public StatMod(string name,int modifier)
            {
                Name = name;
                //Type = type;
                Modifier = modifier;
            }
        }

        public StatType Type;
        private int Max;
        //TODO: should decreases like damage be done with modifiers instead?
        
        //TODO: modifiers should have a func<bool> which determines the reason they are valid
        public List<StatMod> Modifiers;

        public Stat(StatType type, int max)
        {
            Type = type;
            Max = max;
            Modifiers = new List<StatMod>();
        }

        public int GetStatMax()
        {
            int x = Max + Modifiers.Sum(mod => mod.Modifier);

            return x;// % 1 < 0.5f ? (int)x : (int)x + 1;
        }

        //accumulation first and then each multiplier is done at the base
        public string GetStatDescription()
        {
            string x = Type + ": " + Max + "(base)";

            foreach (var mod in Modifiers)
            {
                x += " + " + mod.Modifier.ToString("F") + "(" + mod.Name + ")";
            }

            x += " = " + GetStatMax();

            return x;
        }

        public void LevelUp()
        {
            Max++;
            //Debug.Log(Name + " increased to "+ Max);
        }
    }


    [HideInInspector] public Lootable LootTarget;
    [HideInInspector] public PlayerTeam Team;

    //TODO: use these!! remove the fields
    [Serializable]
    public enum StatType
    {
        DAMAGE, AIM, ATTENTION, COURAGE, HEALTH, SPEED, SMARTS
        , COUNT
    }

    [Header("Stats")]

    //TODO: use a max for stats;
    public Stat DMG;
    public Stat AIM;
    public Stat ATT;
    public Stat COU;
    public Stat HEA;
    public Stat SPE;
    public Stat SMA;
    [SerializeField]
    public Dictionary<StatType,Stat> Stats;

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


    [Header("Movement")]

    public int idleDistance;
    public bool Walking;
    public float AttackRange;

    public bool actionInProgress;

    [HideInInspector]
    //should ignore z for 2d.
    //public Vector3 Target;

    public class TargetDeathEvent : UnityEvent{ }
    public TargetDeathEvent OnTargetDeath = new TargetDeathEvent();
    
    public class AttackEvent : UnityEvent<Character> { }
    public AttackEvent OnAttackCharacter = new AttackEvent();
    public AttackEvent OnBeingAttacked = new AttackEvent();

    public Vector3 Target;
    public NavMeshAgent navMeshAgent;

    public EquipmentManager EquipmentManager;
    public Dictionary<Equipment.EquipLocations,Equipment> Equipped = new Dictionary<Equipment.EquipLocations, Equipment>();

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
            if (HealtBar)
                HealtBar.HealthImage.fillAmount = value / (float)HEA.GetStatMax();
            if (value == _health) return;
            if (value <= 0)
                OnDeath.Invoke(this);
            if (value < _health)
                OnDamage.Invoke(_health - value);
            _health = value;
        }
    }

    public HealtBar HealtBar;

    //TODO: all typical values should have a modifier List with val, modType and modName, which makees it easier to add and remove modifiers from equipment and stuff. 
    //TODO: also values can be easily shown as a total or with all modifier displayed
    public float AttackTime;

    [Header("Moral stats")]
    private int _morale = 10;

    //TODO: use a max moral and have a current of the stat
    public int Morale
    {
        get { return _morale; }
        set
        {
            if (value == _morale) return;
            _morale = value;
            //Debug.Log(gameObject.name + " lost " + value + " moral");
            if (_morale <= 0)
            {
                Debug.Log(name+" fleeing now!");
                actionInProgress = false;
                ChangeState(CharacterState.Fleeing,true);
            }
            else if(Fleeing())
            {
                ChangeState(CharacterState.Idling);
            }
        }
    }

    //maybe hide all these in inspector:

    public float MoralLossModifier = 1f;
    public int MoralLossOnFriendDeath = 5;
    public int MoralBoostOnEnemyDeath = 5;
    public int MoralBoostOnKill = 5;

    public float AmbushModifier = 1.2f;

    [Header("Sprite")]
    //public SpriteRenderer CharacterSprite;
    public Color DamageColor, NormalColor;

    public Material Material;
    
    public class DamageEvent : UnityEvent<int> { }
    public DamageEvent OnDamage = new DamageEvent();

    //using self as parameter, so other listeners will know who dead
    public class DeathEvent : UnityEvent<Character> { }
    public  DeathEvent OnDeath = new DeathEvent();

    [Header("Animation")]
    public Animator Animator;
    public float SpeedAnimationThreshold;

    private Vector2 smoothDeltaPosition = Vector2.zero;
    private Vector2 velocity = Vector2.zero;


    private const string FLEE_ANIMATION_BOOL = "Fleeing";
    private const string DEATH_ANIMATION_BOOL = "Dead";
    private const string ATTACK_ANIMATION_BOOL = "Attacking";
    private const string RANGED_ATTACK_ANIMATION_BOOL = "ArcherAttack";
    private const string MOVE_ANIMATION_BOOL = "Walking";
    private const string IDLE_ANIMATION_BOOL = "Idling";


    private Collider2D coll;
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

    public int IrritationMeter = 0;
    public int IrritaionTolerance = 50;

    public int provokeDistance = 10;

    public Area InArea;
    private Area fleeingToArea;
    private Coroutine agentStuckRoutine;

    public void Start()
    {
        //if(!Voice)
        //    Voice = GetComponentInChildren<AudioSource>();
        if (Voice)
        {
            VoicePitch = Random.Range(PitchMin, PitchMax);

            if (CharacterRace == Race.Zombie)
                VoicePitch /= 2;

            Voice.pitch = VoicePitch;
        }

        if (!HealtBar)
            HealtBar = GetComponentInChildren<HealtBar>();

        for (int i = 0; i < (int)Equipment.EquipLocations.COUNT; i++)
        {
            Equipped.Add((Equipment.EquipLocations)i,null);
        }


        if (HasEquipment)
        {
            Equip(EquipmentGen.GetRandomEquipment());
        }

        //------------------------- STAT SET-UP --------------------------
        DMG = new Stat(StatType.DAMAGE, Random.Range(DamMin, DamMax));
        AIM = new Stat(StatType.AIM, Random.Range(AimMin, AimMax));
        ATT = new Stat(StatType.ATTENTION, Random.Range(AttMin, AttMax));
        COU = new Stat(StatType.COURAGE, Random.Range(CouMin, CouMax));
        SPE = new Stat(StatType.SPEED, Random.Range(SpeMin, SpeMax));
        SMA = new Stat(StatType.SMARTS, Random.Range(SmaMin, SmaMax));
        Stats = new List<Stat>() {DMG,AIM,ATT,COU,SPE,SMA}.ToDictionary(s=> s.Type);

        //Health is a special case
        HEA = new Stat(StatType.HEALTH, Random.Range(HeaMin, HeaMax));
        Health = HEA.GetStatMax();
        
        Material = GetComponentInChildren<Renderer>().material;
        if(Material &&Material.HasProperty("_Color"))
            NormalColor = Material.color;
        DamageColor = Color.red;
        
        OnDamage.AddListener(x=> StartCoroutine(HurtRoutine()));
        OnDeath.AddListener(Die);
        
        AttackRange = transform.lossyScale.x * 2f;

        OnTargetDeath.AddListener(TargetGone);
        OnBeingAttacked.AddListener(BeingAttacked);
        OnAttackCharacter.AddListener(AttackCharacter);

        if(!navMeshAgent)
            navMeshAgent = GetComponentInChildren<NavMeshAgent>();

        //navMeshAgent.speed = SPE.GetStatMax() /2f; Set in fixedupdate
        Morale = COU.GetStatMax();

        if(!navMeshAgent) Debug.LogWarning(name+ ": character does not have Nav Mesh Agent");
    }

    protected void FixedUpdate()
    {
        HandleAnimation();

        if (!Alive() || !GameManager.Instance.GameStarted)
            return;
        

        if (InArea && InArea.Visible() && MovementAudio && !MovementAudio.isPlaying)
            MovementAudio.Play();
        else if(InArea && !InArea.Visible() && MovementAudio && MovementAudio.isPlaying)
            MovementAudio.Stop();

        navMeshAgent.speed = SPE.GetStatMax() / 2f;

        DesiredVelocity = navMeshAgent.desiredVelocity.sqrMagnitude;
        AgentVelocity = navMeshAgent.velocity.sqrMagnitude;
        HasPath = navMeshAgent.hasPath;
        PathStale = navMeshAgent.isPathStale;
        IsOnNavMesh = navMeshAgent.isOnNavMesh;

        if (IncoherentNavAgentSpeed() && agentStuckRoutine == null)
            agentStuckRoutine = StartCoroutine(CheckForNavAgentStuck(0.1f));

        //TODO: merge together with move's switch statement
        if (Attacking() && AttackTarget && AttackTarget.Alive() && InAttackRange()
        ) //has live enemy target and in attackrange
        {
            navMeshAgent.isStopped = true;

            if (_attackRoutine == null)
                _attackRoutine = StartCoroutine(AttackRoutine());
        }
        else
        {
            navMeshAgent.isStopped = false;
            SelectAction();
        }
    }


    //TODO: override in goblin class.
    private void HandleAnimation()
    {
        if (!Animator) return;

        //Animator.SetFloat("Speed", navMeshAgent.desiredVelocity.magnitude);

        //Vector3 worldDeltaPosition = navMeshAgent.nextPosition - transform.position;

        //// Map 'worldDeltaPosition' to local space
        //float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        //float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        //Vector2 deltaPosition = new Vector2(dx, dy);

        //// Low-pass filter the deltaMove
        //float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        //smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        //// Update velocity if time advances
        //if (Time.deltaTime > 1e-5f)
        //    velocity = smoothDeltaPosition / Time.deltaTime;

        //bool shouldMove = velocity.magnitude > 0.5f && navMeshAgent.remainingDistance > navMeshAgent.radius;

        //// Update animation parameters
        ////Animator.SetBool("move", shouldMove);
        //Animator.SetFloat("velx", velocity.x);
        //Animator.SetFloat("vely", velocity.y);

        //GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;

        if (navMeshAgent)
            Animator.SetFloat("Speed",navMeshAgent.speed);

        if (!Alive())
        {
            Animate(DEATH_ANIMATION_BOOL);
        }
        else if (Fleeing())
        {
            Animate(FLEE_ANIMATION_BOOL);
        }
        else if (_attackRoutine != null)
        {
            Animate(Equipped.Values.Any(e => e && e.Type == Equipment.EquipmentType.Bow)
                ? RANGED_ATTACK_ANIMATION_BOOL
                : ATTACK_ANIMATION_BOOL);
        }
        else if (navMeshAgent.desiredVelocity.sqrMagnitude > SpeedAnimationThreshold)
        {
            Animate(MOVE_ANIMATION_BOOL);
        }
        else
        {
            Animate(IDLE_ANIMATION_BOOL);
        }
    }

    /// <summary>
    /// to be used for orders
    /// will do nothing if state == dead
    /// </summary>
    /// <param name="newState">The new character state</param>
    public void ChangeState(CharacterState newState, bool immedeately = false)//, int leaderAncinitet = 10)
    {
        if(!Alive())
            return;
        
        //TODO: check if state is already being changed
        StartCoroutine(immedeately
            ? StateChangingRoutine(newState, 0)
            : StateChangingRoutine(newState, Random.Range(0.2f, 2f)));
    }

    private IEnumerator StateChangingRoutine(CharacterState newState, float wait)
    {
        yield return new WaitForSeconds(wait);

        //TODO: maybe sounds on specific states
        //if (Voice&& !Voice.isPlaying)
        //    Voice.PlayOneShot(SoundBank.GetSound(SoundBank.GoblinSound.Grunt));
        
        if (State != CharacterState.Dead)
            State = newState;
        
        actionInProgress = false;

        if (State == CharacterState.Attacking)
        {
            yield return new WaitForSeconds(Random.Range(0f,1.5f));
            if(State == CharacterState.Attacking)
                Speak(SoundBank.GoblinSound.Roar);
        }

    }

    public bool Travelling()
    {
        return State == CharacterState.Travelling;
    }

    public bool Attacking()
    {
        return State == CharacterState.Attacking;
    }

    public bool Fleeing()
    {
        return State == CharacterState.Fleeing;
    }
    public bool Idling()
    {
        return State == CharacterState.Idling;
    }

    public bool Hiding()
    {
        return State == CharacterState.Hiding;
    }


    public bool Equip(Equipment e)
    {
        if (!Equipped.ContainsKey(e.EquipLocation))
        {
            Debug.LogWarning("not a equip location: " + e.EquipLocation);
            return false;
        }
        if (Equipped[e.EquipLocation] != null)
        {
            Debug.LogWarning("already equipped at "+ e.EquipLocation);
            return false;
        }
        if (this as Goblin && !e.IsUsableby(this as Goblin))
        {
            Debug.LogWarning(gameObject.name + ": Not usable by " + ((Goblin)this).ClassType);
            return false;
        }
        
        //Debug.Log("Equipped "+ e.name + " to " + name);

        Equipped[e.EquipLocation] = e;

        e.OnEquip.Invoke(this);

        return true;
    }

    public void RemoveEquipment(Equipment e)
    {
        if (!Equipped.ContainsKey(e.EquipLocation))
        {
            Debug.LogError("not a equip location: " + e.EquipLocation);
            return;
        }
        if (!Equipped[e.EquipLocation])
        {
            Debug.Log("not equipped at " + e.EquipLocation);
            return;
        }
        Equipped[e.EquipLocation] = null;

        e.OnDeequip.Invoke(this);
    }

    #region Private methods

    //TODO: make generic methods for this and other getclosests
    private Hidable GetClosestHidingPlace()
    {
        //get these from a game or fight controller instead for maintenance
        //GameObject[] gos;
        //gos = GameObject.FindGameObjectsWithTag("Hidable");

        Hidable closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (Hidable go in InArea.Hidables)//.Where(h=> h.GetComponent<Hidable>().Area = InArea))
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
        var enemyTag = playerChar ? "Enemy" : "Player";

        if (!InArea)
        {
            Debug.LogWarning(name + " is not in area!");
            return null;
        }

        //get these from a game or fight controller instead for maintenance
        var gos = InArea.PresentCharacters.Where(c => c.tag == enemyTag && c.Alive());//GameObject.FindGameObjectsWithTag(enemyTag).Select(g=>g.GetComponent<Character>());
        Character closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (var go in gos.Where(e=>e.InArea == InArea))
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
        return closest;
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
        if(Voice && !Voice.isPlaying)
            Speak(SoundBank.GoblinSound.Roar);

        target.OnBeingAttacked.Invoke(this);
    }

    private void BeingAttacked(Character attacker)
    {
        if(Fleeing() || Surprised()) return;

        if (!Attacking() || AttackTarget == null)
        {
            State = CharacterState.Attacking;
            var closest = GetClosestEnemy();
            
            AttackTarget = closest ? closest : attacker;
        }
    }

    private bool Surprised()
    {
        return State == CharacterState.Surprised;
    }

    private IEnumerator AttackRoutine()
    {
        //Debug.Log(gameObject.name + " is Attacking " + AttackTarget.gameObject.name);
        
        State = CharacterState.Attacking;
        while (Attacking() && InAttackRange() && AttackTarget.Alive())
        {
            Speak(SoundBank.GoblinSound.Attacking);


            //HIT TARGET
            var damage = Random.Range(1, DMG.GetStatMax());
            if (AttackTarget.Surprised())
                damage = (int)(damage * AmbushModifier);
            var target = AttackTarget;
            if(!(target.Team && GameManager.Instance.InvincibleMode))
                target.Health -= damage;

            if (target.Health <= 0)
            {
                //Debug.Log(name + " killed " + target.name);

                if (this as Goblin)
                {
                    ((Goblin) this).Xp += GameManager.XpKill();
                    if (Team)
                        Team.AddXp(GameManager.XpTeamKill());
                    Speak(SoundBank.GoblinSound.Laugh);
                }

                break;

            }

            //Debug.Log(gameObject.name + " hit " + AttackTarget.gameObject.name +" for " + Damage + " damage");

            //should be tied to animation maybe?
            yield return new WaitForSeconds(AttackTime);
        }

        _attackRoutine = null;
    }

    public bool Alive()
    {
        return State != CharacterState.Dead;
    }

    private  void SelectAction()
    {

        switch (State)
        {
            case CharacterState.Idling:
                //reset morale
                Morale = COU.GetStatMax();

                if (actionInProgress)
                {
                    if (navMeshAgent.remainingDistance < 0.02f)
                        actionInProgress = false;
                }
                else if (IrritationMeter >= IrritaionTolerance)
                {
                    ChangeState(CharacterState.Attacking);
                }
                else if (Random.value < 0.015f) //selecting idle action
                {
                    actionInProgress = true;

                    Vector3 dest;

                    if (InArea)
                    {
                        if (GetClosestEnemy() 
                            && ( //ANY friends fighting
                            InArea.PresentCharacters.Any(c => c.tag == tag && c.Alive() && c.Attacking())
                            // I am aggressive wanderer
                            || (StickToRoad && InArea.PresentCharacters.Any(c => c.tag == "Player" &! c.Hiding()))
                            ))
                        {
                            //Debug.Log(name + ": Joining attack without beeing attacked");

                            ChangeState(CharacterState.Attacking,true);
                            Morale -= 5;
                            Target = GetClosestEnemy().transform.position;
                            dest = Target;
                        }
                        else if ((this as Goblin) && Team && Team.Leader.InArea != InArea && Team.Leader.Idling())
                        {
                            dest = Team.Leader.InArea.GetRandomPosInArea();
                        }
                        else if ((this as Goblin) &&tag == "Player" && GetClosestEnemy() && (GetClosestEnemy().transform.position - transform.position).magnitude < provokeDistance)
                        {
                            ChangeState(CharacterState.Provoking, true);
                            var ctx = GetClosestEnemy();
                            (this as Goblin).ProvokeTarget = ctx;
                            Speak(SoundBank.GoblinSound.Laugh);
                            dest = ctx.transform.position;
                        }
                        else if (StickToRoad)
                        {
                            var goingTo = InArea.GetClosestNeighbour(transform.position,true);

                            dest = goingTo.PointOfInterest ? goingTo.GetRandomPosInArea(): goingTo.transform.position;
                            
                            //Debug.Log(name + ": Wandering to "+ goingTo);
                            Target = dest;
                            
                            goingTo.PresentCharacters.ForEach(c => StartCoroutine(c.SpotArrivalCheck(this)));

                            ChangeState(CharacterState.Travelling,true);
                        }
                        else
                            dest = InArea.GetRandomPosInArea();
                    }
                    else
                    {
                        dest = transform.position + Random.insideUnitSphere * idleDistance;
                        dest.y = 0;
                    }

                    navMeshAgent.SetDestination(dest);//new Vector3(Random.Range(-idleDistance, idleDistance), 0,Random.Range(-idleDistance, idleDistance)));

                    Walking = Random.value < 0.75f;
                }
                //TODO: use a different method for activity selection than else if
                else if (Random.value < 0.0025f && this as Goblin && Team
                    && !(Team.Leader == this) && (this as Goblin).ClassType > Goblin.Class.Slave
                    & !Team.Challenger && (Team.Leader as Goblin).CurrentLevel < ((Goblin)this).CurrentLevel)
                {
                    //TODO: make it only appear after a while

                    Debug.Log("Chief Fight!!");
                    Team.ChallengeForLeadership(this as Goblin);
                }
                //TODO: define better which characters should search stuff
                else if (Random.value < 0.001f * ATT.GetStatMax() && Team && this as Goblin && !InArea.AnyEnemies() && InArea.Lootables.Any(l => !l.Searched))
                {
                    var loots = InArea.Lootables.Where(l => !l.Searched).ToArray();

                    var loot = loots[Random.Range(0, loots.Count())];

                    navMeshAgent.SetDestination(loot.transform.position);

                    LootTarget = loot;

                    State = CharacterState.Searching;

                }
                break;
            case CharacterState.Attacking:
                if (AttackTarget && AttackTarget.Alive() && AttackTarget.InArea == InArea)
                {
                    if (AttackTarget.Fleeing())
                    {
                        var c = GetClosestEnemy();
                        if (c)
                            AttackTarget = c;
                    }

                    navMeshAgent.SetDestination(AttackTarget.transform.position);

                    //TODO: add random factor
                }
                else
                {
                    TargetGone();
                }
                break;
            case CharacterState.Travelling:
                navMeshAgent.SetDestination(Target);
                //check for arrival and stop travelling
                if (Vector3.Distance(transform.position, Target) < 3f)
                {
                    //Debug.Log(name +" arrived at target");
                    State = CharacterState.Idling;
                    actionInProgress = false;
                    break;
                }

                break;
            case CharacterState.Fleeing:
                Speak(SoundBank.GoblinSound.PanicScream);

                //if (actionInProgress &! navMeshAgent.hasPath)
                //{
                //    //TODO: move into next if statement, if correct
                //    Debug.Log("stuck fleeing resolved");
                //    actionInProgress = false;
                //    ChangeState(CharacterState.Idling, true);
                //}
                if (fleeingToArea == InArea && navMeshAgent.remainingDistance < 0.1f )
                {
                    actionInProgress = false;
                    ChangeState(CharacterState.Idling,true);
                }
                else if (!actionInProgress)
                {
                    fleeingToArea = InArea.GetClosestNeighbour(transform.position,StickToRoad);
                    navMeshAgent.SetDestination(fleeingToArea.GetRandomPosInArea());

                    Walking = false;
                    actionInProgress = true;
                }
                break;
            case CharacterState.Dead:
                break;
            case CharacterState.Hiding:
                if (!Hidingplace)
                {
                    State = CharacterState.Idling;
                }
                //already set the destination
                if ((navMeshAgent.destination - hiding.HideLocation.transform.position).sqrMagnitude < 1f)
                {
                    if (InArea.AnyEnemies() && navMeshAgent.remainingDistance > 0.2f)
                    {
                        ChangeState(CharacterState.Idling);
                    }
                }
                else
                {
                    navMeshAgent.SetDestination(hiding.HideLocation.transform.position);
                }
                break;
                //Only to be used for chief fights. TODO: rename
            case CharacterState.Watching:
                if (!Team || !Team.Challenger)
                {
                    //cheer
                    Speak(SoundBank.GoblinSound.Laugh);
                    ChangeState(CharacterState.Idling, true);
                }
                else
                {
                    if (Vector3.Distance(transform.position, Team.Challenger.transform.position) < 3f)
                    {
                        Speak(SoundBank.GoblinSound.Roar);

                        navMeshAgent.ResetPath();
                    }
                    else if (!actionInProgress)
                    {
                        navMeshAgent.SetDestination(Team.Challenger.transform.position);
                        actionInProgress = true;
                    }
                }

                break;
            case CharacterState.Searching:
                //check for arrival and stop travelling
                if (Vector3.Distance(transform.position, LootTarget.transform.position) < 2f)
                {
                    if (LootTarget.ContainsLoot)
                    {
                        Speak(SoundBank.GoblinSound.Laugh);
                        PopUpText.ShowText(name + " found " + LootTarget.Loot);
                        Team.Treasure++;
                    }
                    if (LootTarget.ContainsFood)
                    {
                        Speak(SoundBank.GoblinSound.Laugh);
                        PopUpText.ShowText(name + " found " + LootTarget.Food);
                        Team.Food += 5;
                    }
                    if(this as Goblin)
                        foreach (var equipment in LootTarget.EquipmentLoot)
                        {
                            //TODO: create player choice for selecting goblin
                            Speak(SoundBank.GoblinSound.Laugh);
                            PopUpText.ShowText(name + " found " + equipment.name);
                            if (Team && Team.Members.Count > 1)
                                Team.EquipmentFound(equipment,this as Goblin);
                            else
                                Equip(equipment);
                        }

                    LootTarget.EquipmentLoot.Clear();

                    LootTarget.ContainsFood = false;
                    LootTarget.ContainsLoot = false;
                    LootTarget.Searched = true;

                    State = CharacterState.Idling;
                    break;
                }
                break;
            case CharacterState.Provoking:
                var g = this as Goblin;

                if (!g)
                {
                    Debug.LogWarning("Non-goblin is being provocative");
                    break;
                }

                if (!g.ProvokeTarget || g.ProvokeTarget.InArea != InArea)
                {
                    Speak(SoundBank.GoblinSound.Laugh);
                    g.ProvokeTarget = GetClosestEnemy();
                }

                if (!g.ProvokeTarget)
                {
                    ChangeState(CharacterState.Idling, true);
                    break;
                }

                if (g.ProvokeTarget.Attacking())
                {
                    ChangeState(CharacterState.Attacking);
                }

                if (navMeshAgent.remainingDistance < 0.5f)
                    navMeshAgent.SetDestination(g.ProvokeTarget.transform.position);
                //the closer the more likely they will runaway
                //TODO: this should be handled a lot more elegantly
                else if ((g.ProvokeTarget.transform.position - transform.position).magnitude < 4)
                //(g.ProvokeScaredCurve.Evaluate((g.ProvokeTarget.transform.position - transform.position).magnitude) <Random.value*provokeDistance)
                {
                    //Debug.Log(name + " Running away in provocation");
                    if (Random.value < 0.1f)
                        Speak(SoundBank.GoblinSound.Grunt);

                    if (Random.value < 0.3f)
                        g.ProvokeTarget.IrritationMeter++;

                    //run away
                    //TODO: check that the position is in the area
                    //TODO: use other
                    var dest = InArea.GetRandomPosInArea();

                    navMeshAgent.SetDestination(dest);

                    //navMeshAgent.SetDestination(transform.position +
                    //    (transform.position - g.ProvokeTarget.transform.position).normalized * provokeDistance/2);
                }


                break;
            case CharacterState.Surprised:
                navMeshAgent.isStopped = true;

                if(SurprisedTime +SurprisedStartTime <= Time.time)
                    ChangeState(CharacterState.Attacking);
                break;
            case CharacterState.Resting:


                if (!Team || !Team.Campfire)
                {
                    ChangeState(CharacterState.Idling, true);
                }
                else
                {
                    if (Vector3.Distance(transform.position, Team.Campfire.transform.position) < 4f)
                    {
                        Speak(SoundBank.GoblinSound.Eat);

                        navMeshAgent.ResetPath();
                    }
                    else if (!actionInProgress)
                    {
                        navMeshAgent.SetDestination(Team.Campfire.transform.position);
                        actionInProgress = true;
                    }
                }

                break;
            default:
                break;
        }

    }

    private IEnumerator SpotArrivalCheck(Character character)
    {
        yield return new WaitForSeconds(Random.Range(0.5f,3f));

        //if enemy and not fleeing or fighting and attentive enough
        //TODO: double up chance as scout
        if(!InArea.AnyEnemies() && this as Goblin &! Fleeing() &! Hiding() && Alive() &! Attacking() && character.tag == "Enemy" )
        {
            if (Random.Range(0, 12) < ATT.GetStatMax())
                (this as Goblin).Shout("I see enemy!!", SoundBank.GoblinSound.EnemyComing);
            //else
            //{
            //    Debug.Log(name + " failed enemy spoting");
            //}
            
        }
    }

    private IEnumerator ActionInProgressUntill(Func<bool> p)
    {
        actionInProgress = true;

        yield return new WaitUntil(p);

        actionInProgress = false;
    }

    //could take a damage parameter
    public IEnumerator HurtRoutine()
    {
        if (!Material)
            yield break;

        Speak(SoundBank.GoblinSound.Hurt);

        Material.color = DamageColor;

        yield return new WaitForSeconds(0.1f);

        Material.color = NormalColor;
    }


    private void Die(Character self)
    {
        //TODO: remove listeners
        OnDamage.RemoveAllListeners();

        if(this as Goblin)
            (this as Goblin).particles.Stop();

        Speak(SoundBank.GoblinSound.Death);

        if(MovementAudio)
            MovementAudio.Stop();

        State = CharacterState.Dead;

        HealtBar.gameObject.SetActive(false);

        if (tag == "Player")
            PopUpText.ShowText(name + " is dead");

        //LOOT CREATIONS
        var loot = gameObject.AddComponent<Lootable>();

        loot.ContainsLoot = false;
        if (CharacterRace != Race.Goblin)
        {
            loot.ContainsFood = true;

            loot.Food = CharacterRace + " " + NameGenerator.GetFoodName();
        }
        var removeEquip = Equipped.Values.Where(e => e).ToArray();
            
        foreach (var eq in removeEquip)
        {
            RemoveEquipment(eq);

            loot.EquipmentLoot.Add(eq);

            eq.transform.parent = loot.transform.parent;
        }

        InArea.Lootables.Add(loot);

        //TODO: check if this create problems:
        navMeshAgent.enabled = false;
    }

    public void MoveTo(Area a, bool immedeately = false)
    {
        if (Fleeing() || Attacking())
            return;

        MoveTo(a.GetRandomPosInArea(),immedeately);
    }

    public void MoveTo(Vector3 t, bool immedeately = false)
    {
        ChangeState(Character.CharacterState.Travelling, immedeately);

        Target = t;
        Target = t;

        actionInProgress = true;
    }

    private bool InAttackRange()
    {
        if (!AttackTarget || !AttackTarget.Alive())
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

    //TODO: move this and all references to goblin
    public void Speak(SoundBank.GoblinSound soundtype, bool overridePlaying = false)
    {
        if (InArea.Visible() && Voice && Voice.isActiveAndEnabled && (overridePlaying || !Voice.isPlaying))
            Voice.PlayOneShot(SoundBank.GetSound(soundtype));
    }

    public void Hide()
    {
        Hidingplace = GetClosestHidingPlace();

        if (!Hidingplace) return;
        
        State = CharacterState.Hiding;
    }

    private void Animate(string boolName)
    {
        DisableOtherAnimations(Animator, boolName);
        Animator.SetBool(boolName, true);
    }

    private void DisableOtherAnimations(Animator animator, string animation)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name != animation && parameter.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(parameter.name, false);
            }
        }
    }

    private IEnumerator CheckForNavAgentStuck(float time)
    {
        var start = Time.time;

        while (start + time > Time.time && IncoherentNavAgentSpeed())
        {
            yield return null;
        }
        if(IncoherentNavAgentSpeed())
        {
            //TODO: test that this is working
            Debug.Log(name + ": Bump");
            navMeshAgent.ResetPath();
        }
        agentStuckRoutine = null;
    }

    private bool IncoherentNavAgentSpeed() =>
        (navMeshAgent.desiredVelocity.sqrMagnitude > navMeshAgent.speed/3 && navMeshAgent.velocity.sqrMagnitude < 0.001f);
}
