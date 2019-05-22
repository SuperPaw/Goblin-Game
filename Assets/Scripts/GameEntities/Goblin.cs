using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Goblin : Character
{
    [Header("Goblin Specific")]
    //consider removing to Character
    //meaning how often they notice what other are doing
    //0-10
    public GoblinWarrens Warrens;

    public Image ChiefImage;
    
    public Image StateImage;

    public Character ProvokeTarget;

    [Flags]
    public enum Class
    {
        NoClass = 0,
        Goblin = 1, Slave = 2, Meathshield = 4, Shooter = 8, Ambusher = 16, Scout = 32,
        END = 64,
        ALL = Goblin | Slave | Meathshield | Shooter | Ambusher | Scout
    }

    public Class ClassType;


    private float lastSpeak;
    [SerializeField]
    private float speakWait = 3f;


    [Header("Levelling")]
    public int CurrentLevel = GetLevel(0);
    public static int[] LevelCaps = { 0, 10, 20, 30, 50, 80, 130, 210, 340, 550, 890, 20000 };


    public class LevelUp : UnityEvent { }

    public LevelUp OnLevelUp = new LevelUp();

    public AnimationCurve ProvokeScaredCurve;

    public static int GetLevel(int xp)
    {
        int level = 0;

        while (LevelCaps[level++] <= xp) { }

        return level-1;
    }

    internal bool WaitingOnLevelup()
    {
        return LevelUps < CurrentLevel;
    }

    public ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;


    public int LevelUps = 1;
    public bool WaitingOnClassSelection;
    private float xp = 0;

    public float Xp
    {
        get
        {
            return xp;
        }
        set
        {
            if (value == xp)
                return;
            xp = value;
            var lvl = GetLevel((int)value);
            //TODO: should check for extra level 
            if (lvl > CurrentLevel)
            {
                CurrentLevel++;
                OnLevelUp.Invoke();
            }
        }
    }

        

    new void Start()
    {
        base.Start();
        

        emission = particles.emission;
        
        //TODO: make better or remove
        //StartCoroutine(AwarenessLoop());

        OnLevelUp.AddListener(NextLevel);
    }

    public new void FixedUpdate()
    {
        base.FixedUpdate();

        //TODO: this could be handled with events instead of checking each frame
        if (Team)
        {
            ChiefImage.enabled = Team.Leader == this;

            StateImage.enabled = true;
            StateImage.sprite = GameManager.GetIconImage(State);

        }
        else
        {
            ChiefImage.enabled = false;
            StateImage.enabled = false;
        }

        if(!Alive())
            this.enabled = false;

        emission.enabled = WaitingOnLevelup();//LevelUps > 0|| WaitingOnClassSelection;
    }

    internal void Heal()
    {
        Morale = COU.GetStatMax() *2;
        Health = HEA.GetStatMax();
    }

    private void NextLevel()
    {
        //a sound
        SoundController.PlayLevelup();

        PopUpText.ShowText(name + " has gained a new level!");
        
        //TODO: health should be handled differently than other stuff
        HEA.LevelUp();

        if (CurrentLevel == 2)
            WaitingOnClassSelection = true;
        
        GoblinUIList.UpdateGoblinList();
    }

    public void SelectClass(Class c)
    {
        ClassType = c;
        WaitingOnClassSelection = false;

        //Adding stat modifiers
        switch (c)
        {
            case Class.Goblin:
                break;
            case Class.Slave:
                COU.Modifiers.Add(new Stat.StatMod("Slave", 1));
                break;
            case Class.Meathshield:
                MoralLossModifier = 0.5f;
                OutgoingDmgPct = 0.8f;
                IncomingDmgPct = 0.8f;
                HEA.Modifiers.Add(new Stat.StatMod("Meatshield",2));
                COU.Modifiers.Add(new Stat.StatMod("Meatshield", 1));
                break;
            case Class.Shooter:
                AttackRange *= 5;
                AIM.Modifiers.Add(new Stat.StatMod("Shooter", 2));
                break;
            case Class.Ambusher:
                AmbushModifier = 2f;
                SPE.Modifiers.Add(new Stat.StatMod("Ambusher", 2));
                break;
            case Class.Scout:
                SMA.Modifiers.Add(new Stat.StatMod("Scout", 2));
                break;
            case Class.END:
                break;
            default:
                throw new ArgumentOutOfRangeException("c", c, null);
        }

        foreach (var equipment in Equipped.Values)
        {
            if (equipment && !equipment.IsUsableby(this))
            {
                RemoveEquipment(equipment);
                Team.OnEquipmentFound.Invoke(equipment,this);
            }
        }
    }

    public bool CanEquip(Equipment e) => e.IsUsableby(this) && Equipped[e.EquipLocation] == null;

    //TODO: use struct to combine text to sound
    public void Shout(string speech, SoundBank.GoblinSound goblinSound)//, bool interrupt = false)
    {
        if (Fleeing() &! Team)
            return;


        //TODO: check that we are not already shouting, maybe control with queue, otherwise just stop previuos shout.
        Speak(goblinSound,true);

        StartCoroutine(ShoutRoutine(speech));
    }
    
    public void Speak(PlayerController.Shout shout, bool overridePlaying = false)
    {
        if (InArea.Visible() && Voice && Voice.isActiveAndEnabled && lastSpeak + speakWait < Time.time && (overridePlaying || !Voice.isPlaying))
        {
            StartCoroutine(ShoutRoutine(shout.Speech));

            Voice.PlayOneShot(shout.GoblinSound);

            lastSpeak = Time.time;
        }
    }
    //TODO: inherit this for each type of character to differentiate sound sets
    public void Speak(SoundBank.GoblinSound soundtype, bool overridePlaying = false)
    {
        if (InArea.Visible() && Voice && Voice.isActiveAndEnabled && (overridePlaying || !Voice.isPlaying))
            Voice.PlayOneShot(SoundBank.GetSound(soundtype));
    }

    private IEnumerator ShoutRoutine(string text)
    {
        navMeshAgent.isStopped = true;

        VoiceText.text = text;
        yield return new WaitForSeconds(ShoutTime);

        if(navMeshAgent.isOnNavMesh)
            navMeshAgent.isStopped = false;

        VoiceText.text = "";
    }

    internal void Search(Lootable loot)
    {
        if (!Idling()) return;

        Speak(SoundBank.GoblinSound.Grunt);

        navMeshAgent.SetDestination(loot.transform.position);

        LootTarget = loot;

        ChangeState(CharacterState.Searching, true);
    }

    public void Kill()
    {
        Health = 0;
    }
}
