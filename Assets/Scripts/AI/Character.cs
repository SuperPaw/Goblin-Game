using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Random = UnityEngine.Random;


//TODO: Should be divided into smaller classes
public abstract class Character : MonoBehaviour
{
    [Header("NAV MESH Debug Values")]
    public float AgentVelocity;
    public float DesiredVelocity;
    public Vector3 Destination;
    public bool HasPath;
    public bool PathStale;
    public bool IsOnNavMesh;

    public Coroutine StateRoutine;


    //Should we use different state for travelling and just looking at something clsoe by
    public enum CharacterState
    {
        Idling, Attacking, Travelling, Fleeing, Hiding, Dead,Watching,Searching,Provoking, Surprised, Resting
    }

    public enum Race
    {
        Goblin, Human, Spider, Zombie, Ogre, Wolf,Orc,Elf,
        NoRace
    }
    
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

    [Header("AI Values")]
    public int MaxGroupSize = 4;
    public float SurprisedTime = 4;

    public bool Aggressive;

    public Race CharacterRace;

    public int IrritationMeter = 0;
    public int IrritaionTolerance = 50;
    
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
            string x = GameManager.GetAttributeDescription(Type) + System.Environment.NewLine;

             x += "" + Max + "(base)";

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
        DAMAGE, AIM, COURAGE, HEALTH, SPEED, SMARTS
        , COUNT
    }

    //TODO: use a max for stats;
    [Header("Stats")]
    public Stat DMG;
    public Stat AIM;
    public Stat COU;
    public Stat HEA;
    public Stat SPE;
    public Stat SMA;
    public Dictionary<StatType,Stat> Stats;

    public int DamMin,
        DamMax,
        AimMin,
        AimMax,
        CouMin,
        CouMax,
        HeaMin,
        HeaMax,
        SpeMin,
        SpeMax,
        SmaMin,
        SmaMax;


    [Header("Movement")]
    public int WalkingSpeed = 2;
    public int idleDistance;
    public bool Walking;
    public float AttackRange;
    public Area TravellingToArea;

    public bool actionInProgress;

    #region Event
    [HideInInspector]
    //Static event to handle generic responces to death
    public class RaceDeathEvent : UnityEvent<Race> { }
    public static RaceDeathEvent OnAnyCharacterDeath = new RaceDeathEvent();
    
    public class DamageEvent : UnityEvent<int> { }
    public DamageEvent OnDamage = new DamageEvent();

    public class CharacterEvent : UnityEvent<Character> { }

    //using self as parameter, so other listeners will know who dead
    public CharacterEvent OnDeath = new CharacterEvent();

    public CharacterEvent OnCharacterCharacter = new CharacterEvent();
    public CharacterEvent OnBeingAttacked = new CharacterEvent();

    public class TargetDeathEvent : UnityEvent{ }
    public TargetDeathEvent OnTargetDeath = new TargetDeathEvent();

    public class AreaEvent : UnityEvent<Area> { }
    public AreaEvent OnAreaChange = new AreaEvent();

#endregion

    public Vector3 Target;
    public NavMeshAgent navMeshAgent;
    public GameObject HolderGameObject;

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
            OnCharacterCharacter.Invoke(_attackTarget);
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
            if (_morale <= 0 &! Fleeing())
            {
                //Debug.Log(name+" fleeing now!");
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
    
    public float IncomingDmgPct = 1f;
    public float OutgoingDmgPct = 1f;
    
    [Header("Sprite")]
    //public SpriteRenderer CharacterSprite;
    public Color DamageColor, NormalColor;

    public Material Material;
    
    [Header("Animation")]
    public Animator Animator;
    public float SpeedAnimationThreshold;
    public ParticleSystem HitParticles, DeathParticles;

    private Vector2 smoothDeltaPosition = Vector2.zero;
    private Vector2 velocity = Vector2.zero;

    private int lastAnimationRandom;

    private const string FLEE_ANIMATION_BOOL = "Fleeing";
    private const string DEATH_ANIMATION_BOOL = "Dead";
    private const string ATTACK_ANIMATION_BOOL = "Attacking";
    private const string RANGED_ATTACK_ANIMATION_BOOL = "ArcherAttack";
    private const string MOVE_ANIMATION_BOOL = "Walking";
    private const string IDLE_ANIMATION_BOOL = "Idling";
    private const string RUN_ANIMATION_BOOL = "Running";
    private const string HIDE_ANIMATION_BOOL = "Hiding";
    private const string CHEER_ANIMATION_BOOL = "Cheering";
    private const string SURPRISE_ANIMATION_BOOL = "Surprised";
    private const string EAT_ANIMATION_BOOL = "Eating";
    private const string PROVOKE_ANIMATION_BOOL = "Provoking";
    private const string PICKUP_ANIMATION_BOOL = "PickUp";


    private Collider2D coll;
    public bool attackAnimation;
    public Hidable hiding;
    
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

    //TODO: move to goblin
    public int provokeDistance = 10;
    //From walking toward to running away

    private Area area;

    public Area InArea
    {
        get { return area; }
        set
        {
            if(value != area)
                OnAreaChange.Invoke(value);
            area = value;
        }
    }

    private Area fleeingToArea;
    private Coroutine stateChangeRoutine;
    
    [Header("Debug")]
    public TextMeshProUGUI DebugText;

    public void Awake()
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

        //TODO: remove this and actually handle the warnings :P
        if(Animator)
            Animator.logWarnings = false;

        if (!HealtBar)
            HealtBar = GetComponentInChildren<HealtBar>();

        for (int i = 0; i < (int) Equipment.EquipLocations.COUNT; i++)
        {
            Equipped.Add((Equipment.EquipLocations) i, null);
        }


        if (HasEquipment)
        {
            Equip(EquipmentGen.GetRandomEquipment());
        }

        //------------------------- STAT SET-UP --------------------------
        DMG = new Stat(StatType.DAMAGE, Random.Range(DamMin, DamMax));
        AIM = new Stat(StatType.AIM, Random.Range(AimMin, AimMax));
        COU = new Stat(StatType.COURAGE, Random.Range(CouMin, CouMax));
        SPE = new Stat(StatType.SPEED, Random.Range(SpeMin, SpeMax));
        SMA = new Stat(StatType.SMARTS, Random.Range(SmaMin, SmaMax));
        Stats = new List<Stat>() {DMG, AIM, COU, SMA}.ToDictionary(s => s.Type);

        //Health is a special case
        HEA = new Stat(StatType.HEALTH, Random.Range(HeaMin, HeaMax));
        Health = HEA.GetStatMax();
    
        Material = GetComponentInChildren<Renderer>().material;
        if(Material &&Material.HasProperty("_Color"))
            NormalColor = Material.color;
        DamageColor = Color.red;
        
        OnDamage.AddListener(x=> StartCoroutine(HurtRoutine()));
        OnDeath.AddListener(Die);
        OnDeath.AddListener(c=> OnAnyCharacterDeath.Invoke(c.CharacterRace));
        
        AttackRange = transform.lossyScale.x * 2f;

        //OnTargetDeath.AddListener(TargetGone);
        OnBeingAttacked.AddListener(BeingAttacked);
        OnCharacterCharacter.AddListener(AttackCharacter);

        if(!navMeshAgent)
            navMeshAgent = GetComponentInChildren<NavMeshAgent>();

        //navMeshAgent.speed = SPE.GetStatMax() /2f; Set in fixedupdate
        Morale = COU.GetStatMax()*2;

        if(!navMeshAgent) Debug.LogWarning(name+ ": character does not have Nav Mesh Agent");

        OnAreaChange.AddListener(AreaChange);

    }

    protected void FixedUpdate()
    {
        if(StateRoutine == null)
            ChangeState(CharacterState.Idling,true);

        if (DebugText )
        {
            DebugText.text = GameManager.Instance.DebugText ? State.ToString(): "";
        }
        
        // Debug Draw
        if(Target != Vector3.zero) Debug.DrawLine(transform.position, Target, Color.blue);
        if (navMeshAgent) Debug.DrawLine(transform.position, navMeshAgent.destination, Color.red);
        if(this as Goblin && (this as Goblin).ProvokeTarget) Debug.DrawLine(transform.position, (this as Goblin).ProvokeTarget.transform.position, Color.cyan);
        if (this as Goblin && (this as Goblin).LootTarget) Debug.DrawLine(transform.position, (this as Goblin).LootTarget.transform.position, Color.yellow);


        HandleAnimation();

        if (!Alive() || !GameManager.Instance.GameStarted || !navMeshAgent.isOnNavMesh)
            return;
        

        if (InArea && InArea.Visible() && MovementAudio && !MovementAudio.isPlaying)
            MovementAudio.Play();
        else if(InArea && !InArea.Visible() && MovementAudio && MovementAudio.isPlaying)
            MovementAudio.Stop();

        if(SPE.GetStatMax() < 1)
            Debug.LogWarning("Speed is 0");

        Walking = State == CharacterState.Travelling || State == CharacterState.Idling;

        navMeshAgent.speed = Walking ? Mathf.Min(WalkingSpeed,SPE.GetStatMax())/2f : SPE.GetStatMax() / 2f;

        Destination = navMeshAgent.destination;
        DesiredVelocity = navMeshAgent.desiredVelocity.sqrMagnitude;
        AgentVelocity = navMeshAgent.velocity.sqrMagnitude;
        HasPath = navMeshAgent.hasPath;
        PathStale = navMeshAgent.isPathStale;
        IsOnNavMesh = navMeshAgent.isOnNavMesh;

        //if (IncoherentNavAgentSpeed() && agentStuckRoutine == null)
        //    agentStuckRoutine = StartCoroutine(CheckForNavAgentStuck(0.25f));

    }


    //TODO: override in goblin class.
    private void HandleAnimation()
    {
        if (!Animator) return;

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
        else if (attackAnimation && Attacking())
        {
            Animate(Equipped.Values.Any(e => e && e.Type == Equipment.EquipmentType.Bow)
                ? RANGED_ATTACK_ANIMATION_BOOL
                : ATTACK_ANIMATION_BOOL);
        }
        else if (Searching() && LootTarget && Vector3.Distance(transform.position, LootTarget.transform.position) < 1.2f)
        {
            Animate(PICKUP_ANIMATION_BOOL);
        }
        else if (AgentVelocity > SpeedAnimationThreshold)
        {
            Animate(Walking ?MOVE_ANIMATION_BOOL: RUN_ANIMATION_BOOL);
        }
        else if (Provoking())
        {
            Animate(PROVOKE_ANIMATION_BOOL);
        }
        else if (Hiding())
        {
            Animate(HIDE_ANIMATION_BOOL);
        }
        else if (Watching() &&  !IsChief() && Team.Challenger != this)
        {
            //Animator.SetLookAtPosition(Team.Leader.transform.position);
            transform.LookAt(Team.Leader.transform);
            Animate(CHEER_ANIMATION_BOOL);
        }
        else if (Surprised())
        {
            Animate(SURPRISE_ANIMATION_BOOL);
        }
        else if (Resting())
        {
            Animate(EAT_ANIMATION_BOOL);
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
    public void ChangeState(CharacterState newState, bool immedeately = false, float time = 0f)//, int leaderAncinitet = 10)
    {
        if(!Alive())
            return;
        
        //check if state is already being changed
        //if(stateChangeRoutine != null)
        //    Debug.Log(name + ": Changing State already: newState "+newState + ", old: "+ State);

        stateChangeRoutine = StartCoroutine(
            immedeately? 
            StateChangingRoutine(newState, 0)
            : time > 0f ? 
            StateChangingRoutine(newState, time)
            : StateChangingRoutine(newState, Random.Range(1.5f, 4f)));
    }

    private IEnumerator StateChangingRoutine(CharacterState newState, float wait)
    {
        var fromState = State;
        
        yield return new WaitForSeconds(wait);

        if (fromState != State)
        {
            Debug.Log(name + " no longer "+ fromState + "; Now: "+ State + "; Not " + newState);
            yield break;
        }

        //TODO: maybe sounds on specific states
        //if (Voice&& !Voice.isPlaying)
        //    Voice.PlayOneShot(SoundBank.GetSound(SoundBank.GoblinSound.Grunt));

        if (State == newState)
        {
            Debug.Log(name + " is already "+ newState);
            //yield break;
        }

        if (Morale <= 0 && newState != CharacterState.Fleeing)
        {
            //Debug.Log(name + " not able to change state to " + newState + " Fleeing!");
            yield break;
        }

        if (State != CharacterState.Dead)
            State = newState;

        actionInProgress = false;

        if (newState != CharacterState.Travelling && newState != CharacterState.Attacking &&
            newState != CharacterState.Fleeing)
            TravellingToArea = null;

        Debug.Log($"Starting state: {newState}");
        //TODO: Assign to field and close the last state

        //TODO: just assign and set to null when applicable
        var s = ActionStateProcessor.CreateStateRoutine(this, newState);
        if (s != null)
            StateRoutine = s;
        
        if (this as Goblin && PlayerController.IsStateChangeShout(State))
        {
            (this as Goblin)?.Speak(PlayerController.GetStateChangeReaction(State));
        }
        else if (State == CharacterState.Attacking)
        {
            Aggressive = true;
            yield return new WaitForSeconds(Random.Range(0f,1.5f));
            if(State == CharacterState.Attacking )
                (this as Goblin)?.Speak(SoundBank.GoblinSound.Roar);
        }

        lastAnimationRandom = Random.Range(0, 4);
        Animator.SetInteger("AnimationRandom",lastAnimationRandom);

        stateChangeRoutine = null;
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
    public bool Provoking()
    {
        return State == CharacterState.Provoking;
    }

    public bool Searching() => State == CharacterState.Searching;

    public bool Watching()
    {
        return State == CharacterState.Watching;
    }

    private bool Surprised()
    {
        return State == CharacterState.Surprised;
    }

    private bool Resting() => State == CharacterState.Resting;



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
        var area = TravellingToArea == Team.Leader.InArea ? TravellingToArea : InArea;

        Hidable closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (Hidable go in area.Hidables)//.Where(h=> h.GetComponent<Hidable>().Area = InArea))
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

    public Character GetClosestEnemy()
    {
        var playerChar = (gameObject.tag == "Player");
        var enemyTag = playerChar ? "Enemy" : "Player";

        if (!InArea)
        {
            Debug.LogWarning(name + " is not in area!");
            return null;
        }

        //get these from a game or fight controller instead for maintenance
        var gos = InArea.PresentCharacters.Where(c => c.tag == enemyTag && c.Alive()).ToList();//GameObject.FindGameObjectsWithTag(enemyTag).Select(g=>g.GetComponent<Character>());
        if(TravellingToArea) gos.AddRange(TravellingToArea.PresentCharacters.Where(c => c.tag == enemyTag && c.Alive()));

        Character closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (var go in gos)
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


    //Should only be called through setting the attack target
    private void AttackCharacter(Character target)
    {
        if(Voice && !Voice.isPlaying)
            (this as Goblin)?.Speak(SoundBank.GoblinSound.Roar);

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

    public void AttackEvent()
    {
        if (!Attacking() || !InAttackRange() || !AttackTarget.Alive() || AIM.GetStatMax() < Random.value) return;

        (this as Goblin)?.Speak(SoundBank.GoblinSound.Attacking);

        //HIT TARGET
        var damage = Random.Range(1, DMG.GetStatMax()) * OutgoingDmgPct;

        //TODO: create source for fx and getsound method for effects
        if(Voice)
            Voice.PlayOneShot(SoundBank.GetSound(SoundBank.FXSound.Hit));
        
        if(HitParticles)
            HitParticles.Play(true);

        if (AttackTarget.Surprised())
            damage = (int)(damage * AmbushModifier);
        var target = AttackTarget;
        if (!(target.Team && GameManager.Instance.InvincibleMode))
            target.Health -= (int)Mathf.Round(damage * target.IncomingDmgPct);

        if (target.Health <= 0)
        {
            Debug.Log(name + " killed " + target.name);

            if (this as Goblin)
            {
                ((Goblin)this).Xp += GameManager.XpKill();
                if (Team)
                    Team.OnTeamKill.Invoke();
                (this as Goblin)?.Speak(SoundBank.GoblinSound.Laugh);
            }
        }

        //Debug.Log(gameObject.name + " hit " + AttackTarget.gameObject.name +" for " + damage + " damage");
    }

    public bool Alive()
    {
        return State != CharacterState.Dead;
    }

    public void SpotArrival(Character ch)
    {
        StartCoroutine(SpotArrivalCheck(ch));
    }

    private IEnumerator SpotArrivalCheck(Character character)
    {
        yield return new WaitForSeconds(Random.Range(0.5f,3f));

        if (InArea == null)
        {
            Debug.LogError(name + " not in Area");
            yield break;     }

        //if enemy and not fleeing or fighting and attentive enough
        //TODO: double up chance as scout
        if(!InArea.AnyEnemies() && this as Goblin &! Fleeing() &! Hiding() && Alive() &! Attacking() && character.tag == "Enemy" )
        {
            if (Random.Range(0, 12) < SMA.GetStatMax())
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

        (this as Goblin)?.Speak(SoundBank.GoblinSound.Hurt);

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

        Debug.Log("death sound!!!!");
        (this as Goblin)?.Speak(SoundBank.GoblinSound.Death);

        if(MovementAudio)
            MovementAudio.Stop();

        State = CharacterState.Dead;

        HealtBar.gameObject.SetActive(false);

        //if (tag == "Player")
        //    PopUpText.ShowText(name + " is dead");

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

    private void AreaChange(Area a)
    {
        if (tag == "Player" && IsChief())
        {
            PlayerController.RevealArea(a);

            a.Visited = true;
            foreach (var aggressive in a.PresentCharacters.Where(e => e.tag == "Enemy" && e.Aggressive && e.tag != tag))
            {
                aggressive.ChangeState(Character.CharacterState.Attacking);
            }

        }

        IrritationMeter = 0;

        Morale = COU.GetStatMax() * 2;

        if (Fleeing() || Travelling())
        {
            ChangeState(Character.CharacterState.Idling);
        }

        //if (GameManager.Instance.DisableUnseenCharacters)
        //    HolderGameObject.SetActive(Team || a.Visible());
        
        //TODO: move to area change method
        if (this as Goblin & !IsChief())
        {
            if (a.PointOfInterest)
                (this as Goblin)?.Speak(PlayerController.GetLocationReaction(a.PointOfInterest.PoiType));
            else if (a.AnyEnemies()) //TODO:select random enemy
                (this as Goblin)?.Speak(PlayerController.GetEnemyReaction(a.PresentCharacters.First(ch => ch.tag == "Enemy" && ch.Alive()).CharacterRace));
        }

    }

    public void MoveTo(Area a, bool immedeately = false)
    {
        if (Fleeing() || Attacking())
            return;

        TravellingToArea = a;
        MoveTo(a.GetRandomPosInArea(),immedeately);
    }

    public void MoveTo(Vector3 t, bool immedeately = false)
    {
        ChangeState(Character.CharacterState.Travelling, immedeately);
        
        Target = t;

        actionInProgress = true;
    }
    
    //TODO: move to attack action
    public bool InAttackRange()
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


    public void Hide()
    {
        Hidingplace = GetClosestHidingPlace();

        if (!Hidingplace) return;

        ChangeState(CharacterState.Hiding, true);
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
            ChangeState(CharacterState.Idling,true);
        }
        //agentStuckRoutine = null;
    }

    public virtual bool IsChief() => false;

    private bool IncoherentNavAgentSpeed() =>
        (navMeshAgent.hasPath && navMeshAgent.desiredVelocity.sqrMagnitude > navMeshAgent.speed/3 && navMeshAgent.velocity.sqrMagnitude < navMeshAgent.desiredVelocity.sqrMagnitude /2);
}
