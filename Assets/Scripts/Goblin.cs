using System;
using System.Collections;
using System.Collections.Generic;
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
    public int Awareness;
    
    public GoblinWarrens Warrens;

    public Image ChiefImage;


    public Image StateImage;

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

    public ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;


    public int WaitingOnLevelUp;
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

    public Character ProvokeTarget;

    public enum Class
    {
        Goblin, Slave, Swarmer, Shooter, Ambusher, Scout,
        END
    }

    public Class ClassType;
        

    new void Start()
    {
        base.Start();
        

        emission = particles.emission;

        Awareness = ATT.GetStatMax();

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

        emission.enabled = WaitingOnLevelUp > 0|| WaitingOnClassSelection;
    }

    internal void Rest()
    {
        Morale = COU.GetStatMax();
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
        
        
        WaitingOnLevelUp++;

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
            case Class.Swarmer:
                MoralLossModifier = 0.5f;
                DMG.Modifiers.Add(new Stat.StatMod("Swarmer",2));
                SMA.Modifiers.Add(new Stat.StatMod("Swarmer", -1));
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
                ATT.Modifiers.Add(new Stat.StatMod("Scout", 2));
                break;
            case Class.END:
                break;
            default:
                throw new ArgumentOutOfRangeException("c", c, null);
        }
    }

    private IEnumerator AwarenessLoop()
    {
        while (true && Alive())
        {
            yield return new WaitForSeconds(Mathf.Max(1, 10 - Awareness));

            CheckWhatOthersAreDoing();
        }
    }
    

    private void CheckWhatOthersAreDoing()
    {
        if (!Team || !Alive())
            return;
        //these could also all be more random dependant on Awareness

        //Debug.Log("checking leader distance ");

        // -------------- CHECK FOR LEADER DISTANCE AND MOVE TO HIM -----------------
    }
    
    //TODO: use struct to combine text to sound
    public void Shout(string speech, SoundBank.GoblinSound goblinSound)//, bool interrupt = false)
    {
        if (Fleeing())
            return;


        //TODO: check that we are not already shouting, maybe control with queue, otherwise just stop previuos shout.
        Speak(goblinSound,true);

        StartCoroutine(ShoutRoutine(speech));
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


}
