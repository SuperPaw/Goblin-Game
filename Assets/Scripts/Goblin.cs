using System.Collections;
    using System.Collections.Generic;
    using UnityEditor.Experimental.UIElements;
    using UnityEngine;
    using UnityEngine.Assertions.Must;
    using UnityEngine.Events;
using UnityEngine.UI;

public class Goblin : Character
{
    [Header("Goblin Specific")]
    //consider removing to Character
    //meaning how often they notice what other are doing
    //0-10
    public int Awareness;
    private int goToLeaderDistance = 3;

    public Image ChiefImage;
    public Image StateImage;

    [Header("Levelling")]
    public int CurrentLevel = GetLevel(0);
    public static int[] LevelCaps = { 0, 10, 20, 30, 50, 80, 130, 210, 340, 550, 890, 20000 };


    public class LevelUp : UnityEvent { }

    public LevelUp OnLevelUp = new LevelUp();

    public static int GetLevel(int xp)
    {
        int level = 0;

        while (LevelCaps[level++] < xp) { }

        return level;
    }

    public ParticleSystem particles;
    private ParticleSystem.EmissionModule emission;


    public bool WaitingOnLevelUp;
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
    public enum Class
    {
        NoClass, Swarmer, Shooter, Ambusher, Scout, Slave, 
        END
    }

    public Class ClassType;
        

    new void Start()
    {
        base.Start();

        emission = particles.emission;

        Awareness = ATT.GetStatMax();

        StartCoroutine(AwarenessLoop());

        OnLevelUp.AddListener(NextLevel);
    }

    public new void FixedUpdate()
    {
        base.FixedUpdate();
        //TODO: this could be handled with events instead of checking each frame

        if(Team)
            ChiefImage.enabled = Team.Leader == this;

        StateImage.sprite = GameManager.GetIconImage(State);

        emission.enabled = WaitingOnLevelUp || WaitingOnClassSelection;
    }


    private void NextLevel()
    {
        //Set some level icon active
        
        //a sound

        //TODO: health should be handled differently than other stuff
        HEA.LevelUp();

        if (CurrentLevel == 2)
            WaitingOnClassSelection = true;
        
        WaitingOnLevelUp = true;
    }

    public void SelectClass(Class c)
    {
        ClassType = c;
        WaitingOnClassSelection = false;

        //TODO: add class modifiers
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
        //TODO: make a readyForOrder() method, instead of checking on each state every time
        if (Team.Leader != this & !Attacking() & !Fleeing() &! Travelling() &&
            (transform.position - Team.Leader.transform.position).magnitude > goToLeaderDistance)
        {
            //Debug.Log(name + " going to leader");
            //TODO: make it disappear from at a certain range. Lost goblin...
            var newDes = Team.Leader.transform.position + new Vector3(Random.Range(-0.1f,0.1f), 0, Random.Range(-0.1f,0.1f));


            navMeshAgent.SetDestination(Team.Leader.transform.position);

        }

    }
}
