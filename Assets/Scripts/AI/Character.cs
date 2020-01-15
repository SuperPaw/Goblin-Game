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
public abstract partial class Character : MonoBehaviour
{
    [Header("NAV MESH Debug Values")]
    public float AgentVelocity;
    public float DesiredVelocity;
    public Vector3 Destination;
    public bool HasPath;
    public bool PathStale;
    public bool IsOnNavMesh;
    public bool IsStopped;

    public Coroutine StateRoutine;


    //Should we use different state for travelling and just looking at something clsoe by
    public enum CharacterState
    {
        Idling, Attacking, Travelling, Fleeing, Hiding, Dead, Watching, Searching, Provoking, Surprised, Resting, StartState
    }

    public enum Race
    {
        Goblin, Human, Spider, Zombie, Ogre, Wolf, Orc, Elf,
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
    protected StateController stateController;
    protected AnimationController _animationController;
    public int lastAnimationRandom;
    public float SpeedAnimationThreshold =0.2f;

    public class Stat
    {
        [Serializable]
        public struct StatMod
        {
            public string Name;
            //public ModType Type;
            public int Modifier;

            public StatMod(string name, int modifier)
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
    public Dictionary<StatType, Stat> Stats;

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

    public class TargetDeathEvent : UnityEvent { }
    public TargetDeathEvent OnTargetDeath = new TargetDeathEvent();

    public class AreaEvent : UnityEvent<Area> { }
    public AreaEvent OnAreaChange = new AreaEvent();

    #endregion

    public Vector3 Target;
    public NavMeshAgent navMeshAgent;
    public GameObject HolderGameObject;

    public EquipmentManager EquipmentManager;
    public Dictionary<Equipment.EquipLocations, Equipment> Equipped = new Dictionary<Equipment.EquipLocations, Equipment>();

    private Character _attackTarget;
    public Character AttackTarget
    {
        get { return _attackTarget; }
        set
        {
            if (_attackTarget == value)
            {
                return;
            }

            _attackTarget = value;
            if (!value)
            {
                return;
            }

            OnCharacterCharacter.Invoke(_attackTarget);
            _attackTarget.OnDeath.AddListener(x => OnTargetDeath.Invoke());

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
            {
                HealtBar.HealthImage.fillAmount = value / (float)HEA.GetStatMax();
            }

            if (value == _health)
            {
                return;
            }

            if (value <= 0)
            {
                OnDeath.Invoke(this);
            }

            if (value < _health)
            {
                OnDamage.Invoke(_health - value);
            }

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
            if (value == _morale)
            {
                return;
            }

            _morale = value;
            //Debug.Log(gameObject.name + " lost " + value + " moral");
            if (_morale <= 0 & !Fleeing() & !(this as Goblin && (this as Goblin).Mood > Goblin.Happiness.Neutral))
            {

                //Debug.Log(name+" fleeing now!");
                stateController.ChangeState(CharacterState.Fleeing, true);
            }
            else if (Fleeing())
            {
                stateController.ChangeState(CharacterState.Idling);
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

    public ParticleSystem HitParticles, DeathParticles;

    private readonly Collider2D coll;
    public bool attackAnimation;
    public Hidable hiding;

    public Hidable Hidingplace
    {
        get { return hiding; }
        set
        {
            if (hiding)
            {
                hiding.OccupiedBy = null;
            }

            if (!value)
            {
                return;
            }

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
            if (value != area)
            {
                OnAreaChange.Invoke(value);
            }

            area = value;
        }
    }

    private readonly Area fleeingToArea;

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
            {
                VoicePitch /= 2;
            }

            Voice.pitch = VoicePitch;
        }

        stateController = new StateController(this);

        var Animator = GetComponent<Animator>();

        //TODO: remove this and actually handle the warnings :P
        if (!Animator)
        {
            Animator.logWarnings = false;
            Debug.Log($"{name}: no animator found on");
        }
        else
            _animationController = new AnimationController(this, Animator);

        if (!HealtBar)
        {
            HealtBar = GetComponentInChildren<HealtBar>();
        }

        for (int i = 0; i < (int)Equipment.EquipLocations.COUNT; i++)
        {
            Equipped.Add((Equipment.EquipLocations)i, null);
        }


        if (HasEquipment)
        {
            var randomEquipment = EquipmentGen.GetRandomEquipment();
            Equip(randomEquipment);
        }

        //------------------------- STAT SET-UP --------------------------
        DMG = new Stat(StatType.DAMAGE, Random.Range(DamMin, DamMax));
        AIM = new Stat(StatType.AIM, Random.Range(AimMin, AimMax));
        COU = new Stat(StatType.COURAGE, Random.Range(CouMin, CouMax));
        SPE = new Stat(StatType.SPEED, Random.Range(SpeMin, SpeMax));
        SMA = new Stat(StatType.SMARTS, Random.Range(SmaMin, SmaMax));
        Stats = new List<Stat>() { DMG, AIM, COU, SMA }.ToDictionary(s => s.Type);

        //Health is a special case
        HEA = new Stat(StatType.HEALTH, Random.Range(HeaMin, HeaMax));
        Health = HEA.GetStatMax();

        Material = GetComponentInChildren<Renderer>().material;
        if (Material && Material.HasProperty("_Color"))
        {
            NormalColor = Material.color;
        }

        DamageColor = Color.red;

        OnDamage.AddListener(x => StartCoroutine(HurtRoutine()));
        OnDeath.AddListener(Die);
        OnDeath.AddListener(c => OnAnyCharacterDeath.Invoke(c.CharacterRace));

        AttackRange = transform.lossyScale.x * 2f;

        //OnTargetDeath.AddListener(TargetGone);
        OnBeingAttacked.AddListener(BeingAttacked);
        OnCharacterCharacter.AddListener(AttackCharacter);

        if (!navMeshAgent)
        {
            navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        }

        //navMeshAgent.speed = SPE.GetStatMax() /2f; Set in fixedupdate
        Morale = COU.GetStatMax() * 2;

        if (!navMeshAgent)
        {
            Debug.LogWarning(name + ": character does not have Nav Mesh Agent");
        }

        OnAreaChange.AddListener(AreaChange);

    }

    private void Start()
    {

        //if (StateRoutine == null && GameManager.Instance.GameStarted)
        stateController.ChangeState(CharacterState.Idling, true);

    }

    protected void FixedUpdate()
    {
        if (DebugText)
        {
            DebugText.text = GameManager.Instance.DebugText ? stateController.State.ToString() : "";
        }

        // Debug Draw
        if (Target != Vector3.zero)
        {
            Debug.DrawLine(transform.position, Target, Color.blue);
        }

        if (navMeshAgent)
        {
            Debug.DrawLine(transform.position, navMeshAgent.destination, Color.red);
        }

        if (this as Goblin && (this as Goblin).ProvokeTarget)
        {
            Debug.DrawLine(transform.position, (this as Goblin).ProvokeTarget.transform.position, Color.cyan);
        }

        if (this as Goblin && (this as Goblin).AttackTarget)
        {
            Debug.DrawLine(transform.position, (this as Goblin).AttackTarget.transform.position, Color.white);
        }

        if (this as Goblin && (this as Goblin).LootTarget)
        {
            Debug.DrawLine(transform.position, (this as Goblin).LootTarget.transform.position, Color.yellow);
        }

        _animationController?.HandleAnimation();

        if (!Alive() || !GameManager.Instance.GameStarted || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        if (InArea && InArea.Visible() && MovementAudio && !MovementAudio.isPlaying)
        {
            MovementAudio.Play();
        }
        else if (InArea && !InArea.Visible() && MovementAudio && MovementAudio.isPlaying)
        {
            MovementAudio.Stop();
        }

        if (SPE.GetStatMax() < 1)
        {
            Debug.LogWarning("Speed is 0");
        }

        Walking = stateController.State == CharacterState.Travelling || stateController.State == CharacterState.Idling;

        navMeshAgent.speed = Walking ? Mathf.Min(WalkingSpeed, SPE.GetStatMax()) / 4f : SPE.GetStatMax() / 4f;

        Destination = navMeshAgent.destination;
        DesiredVelocity = navMeshAgent.desiredVelocity.sqrMagnitude;
        AgentVelocity = navMeshAgent.velocity.sqrMagnitude;
        HasPath = navMeshAgent.hasPath;
        PathStale = navMeshAgent.isPathStale;
        IsOnNavMesh = navMeshAgent.isOnNavMesh;
        IsStopped = navMeshAgent.isStopped;

        //if (IncoherentNavAgentSpeed() && agentStuckRoutine == null)
        //    agentStuckRoutine = StartCoroutine(CheckForNavAgentStuck(0.25f));

    }

    public override string ToString()
    {
        return name;
    }

    public bool IsChallenger()
    {
        return Team.Challenger == this;
    }

    public bool NavigationPathIsStaleOrCompleted()
    {
        return
            navMeshAgent.isPathStale ||
            !navMeshAgent.pathPending &&
               (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
               && (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f);
    }

    internal CharacterState GetState() => stateController.State;

    internal void ChangeState(CharacterState s, bool now = false, float time = 0)
    {
        stateController.ChangeState(s, now,time);
    }


    public bool Travelling()
    {
        return stateController.State == CharacterState.Travelling;
    }

    public bool Attacking()
    {
        return stateController.State == CharacterState.Attacking;
    }

    public bool Fleeing() => stateController.State == CharacterState.Fleeing;

    public bool Idling() => stateController.State == CharacterState.Idling;

    public bool Hiding() => stateController.State == CharacterState.Hiding;

    public bool Provoking() => stateController.State == CharacterState.Provoking;

    public bool Searching() => stateController.State == CharacterState.Searching;

    public bool Watching()
    {
        return stateController.State == CharacterState.Watching;
    }

    private bool Surprised()
    {
        return stateController.State == CharacterState.Surprised;
    }

    private bool Resting() => stateController.State == CharacterState.Resting;



    public bool Equip(Equipment e)
    {
        if (!Equipped.ContainsKey(e.EquipLocation))
        {
            Debug.LogWarning("not a equip location: " + e.EquipLocation);
            return false;
        }
        if (Equipped[e.EquipLocation] != null)
        {
            Debug.LogWarning("already equipped at " + e.EquipLocation);
            return false;
        }
        if (this as Goblin)
        {
            if (!e.IsUsableby(this as Goblin))
            {
                Debug.LogWarning(e + ": Not usable by " + ((Goblin)this).ClassType);
                return false;
            }

            (this as Goblin).OnMoodChange.Invoke(2);
        }

        //Debug.Log("Equipped "+ e.name + " to " + name);

        Equipped[e.EquipLocation] = e;


        e.transform.parent = this.transform;

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
            if (!go)
            {
                Debug.LogError("Hidable object does not have hidable script");
            }

            if (go.OccupiedBy != null)
            {
                continue;
            }

            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        if (!closest)
        {
            return null;
        }

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
        if (TravellingToArea)
        {
            gos.AddRange(TravellingToArea.PresentCharacters.Where(c => c.tag == enemyTag && c.Alive()));
        }

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

        if (!closest)
        {
            return null;
        }

        return closest;
    }


    //Should only be called through setting the attack target
    private void AttackCharacter(Character target)
    {
        if (Voice && !Voice.isPlaying)
        {
            (this as Goblin)?.Speak(SoundBank.GoblinSound.Roar);
        }

        target.OnBeingAttacked.Invoke(this);
    }

    private void BeingAttacked(Character attacker)
    {
        if (Fleeing() || Surprised())
        {
            return;
        }

        if (!Attacking())
        {
            stateController.ChangeState(CharacterState.Attacking, true);
            var closest = GetClosestEnemy();

            AttackTarget = closest ? closest : attacker;
        }
    }

    public void AttackEvent()
    {
        if (!Attacking() || !InAttackRange() || !AttackTarget.Alive() || AIM.GetStatMax() < Random.value)
        {
            return;
        } (this as Goblin)?.Speak(SoundBank.GoblinSound.Attacking);

        //HIT TARGET
        var damage = Random.Range(1, DMG.GetStatMax()) * OutgoingDmgPct;

        //TODO: create source for fx and getsound method for effects
        if (Voice && PlayerController.ObjectIsSeen(transform))
        {
            Voice.PlayOneShot(SoundBank.GetSound(SoundBank.FXSound.Hit));
        }

        if (HitParticles)
        {
            HitParticles.Play(true);
        }

        if (AttackTarget.Surprised())
        {
            damage = (int)(damage * AmbushModifier);
        }

        var target = AttackTarget;
        if (!(target.Team && GameManager.Instance.InvincibleMode))
        {
            target.Health -= (int)Mathf.Round(damage * target.IncomingDmgPct);
        }

        if (target.Health <= 0)
        {
            Debug.Log(name + " killed " + target.name);

            if (this as Goblin)
            {
                ((Goblin)this).Xp += GameManager.XpKill;
                if (Team)
                {
                    Team.OnTeamKill.Invoke();
                } (this as Goblin)?.Speak(SoundBank.GoblinSound.Laugh);
            }
        }

        //Debug.Log(gameObject.name + " hit " + AttackTarget.gameObject.name +" for " + damage + " damage");
    }

    public bool Alive()
    {
        return stateController.State != CharacterState.Dead;
    }

    public void SpotArrival(Character ch)
    {
        StartCoroutine(SpotArrivalCheck(ch));
    }

    private IEnumerator SpotArrivalCheck(Character character)
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 3f));

        if (InArea == null)
        {
            Debug.LogError(name + " not in Area");
            yield break;
        }

        //if enemy and not fleeing or fighting and attentive enough
        //TODO: double up chance as scout
        if (!InArea.AnyEnemies() && this as Goblin & !Fleeing() & !Hiding() && Alive() & !Attacking() && character.tag == "Enemy")
        {
            if (Random.Range(0, 12) < SMA.GetStatMax())
            {
                (this as Goblin).Shout("I see enemy!!", SoundBank.GoblinSound.EnemyComing);
            }
            //else
            //{
            //    Debug.Log(name + " failed enemy spoting");
            //}

        }
    }

    //could take a damage parameter
    public IEnumerator HurtRoutine()
    {
        if (!Material)
        {
            yield break;
        } (this as Goblin)?.Speak(SoundBank.GoblinSound.Hurt);

        Material.color = DamageColor;

        yield return new WaitForSeconds(0.1f);

        Material.color = NormalColor;
    }


    private void Die(Character self)
    {
        //TODO: remove listeners
        OnDamage.RemoveAllListeners();

        if (this as Goblin)
        {
            (this as Goblin).particles.Stop();
        }

        Debug.Log("death sound!!!!");
        (this as Goblin)?.Speak(SoundBank.GoblinSound.Death);

        if (MovementAudio)
        {
            MovementAudio.Stop();
        }

        stateController.ChangeState(CharacterState.Dead,true);

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
                aggressive.stateController.ChangeState(Character.CharacterState.Attacking);
            }

        }

        IrritationMeter = 0;

        Morale = COU.GetStatMax() * 2;

        if (Fleeing() || Travelling())
        {
            stateController.ChangeState(Character.CharacterState.Idling);
        }

        //if (GameManager.Instance.DisableUnseenCharacters)
        //    HolderGameObject.SetActive(Team || a.Visible());

        //TODO: move to area change method
        if (this as Goblin & !IsChief() && Team != null)
        {
            if (a.PointOfInterest)
            {
                (this as Goblin)?.Speak(PlayerController.GetLocationReaction(a.PointOfInterest.PoiType));
            }
            else if (a.AnyEnemies()) //TODO:select random enemy
            {
                (this as Goblin)?.Speak(PlayerController.GetEnemyReaction(a.PresentCharacters.First(ch => ch.tag == "Enemy" && ch.Alive()).CharacterRace));
            }
        }

    }

    public void MoveTo(Area a, bool immedeately = false)
    {
        if (Fleeing() || Attacking())
        {
            return;
        }

        TravellingToArea = a;

        MoveTo(a.GetRandomPosInArea(), immedeately);
    }

    public void MoveTo(Vector3 t, bool immedeately = false)
    {
        Target = t;

        if (!Travelling())
        {
            stateController.ChangeState(CharacterState.Travelling, immedeately);
        }
        else //if already travelling we just update the target
        {
            navMeshAgent.SetDestination(t);
        }
    }

    //TODO: move to attack action
    public bool InAttackRange()
    {
        if (!AttackTarget || !AttackTarget.Alive())
        {
            return false;
        }

        var targetCol = AttackTarget.GetComponent<CapsuleCollider>();

        if (!targetCol)
        {
            Debug.LogWarning(name + "'s target does not have a capsule collider");
            return false;
        }

        var boxCol = GetComponent<CapsuleCollider>();

        return ((boxCol.transform.position - (targetCol.transform.position)).magnitude
            <= boxCol.radius * boxCol.transform.lossyScale.x
            + targetCol.radius * targetCol.transform.lossyScale.x + AttackRange);

    }

    #endregion


    public void Hide()
    {
        Hidingplace = GetClosestHidingPlace();

        if (!Hidingplace)
        {
            return;
        }

        stateController.ChangeState(CharacterState.Hiding, true);
    }



    private IEnumerator CheckForNavAgentStuck(float time)
    {
        var start = Time.time;

        while (start + time > Time.time && IncoherentNavAgentSpeed())
        {
            yield return null;
        }
        if (IncoherentNavAgentSpeed())
        {
            //TODO: test that this is working
            Debug.Log(name + ": Bump");
            stateController.ChangeState(CharacterState.Idling, true);
        }
        //agentStuckRoutine = null;
    }

    public virtual bool IsChief() => false;

    private bool IncoherentNavAgentSpeed() =>
        (navMeshAgent.hasPath && navMeshAgent.desiredVelocity.sqrMagnitude > navMeshAgent.speed / 3 && navMeshAgent.velocity.sqrMagnitude < navMeshAgent.desiredVelocity.sqrMagnitude / 2);
}
