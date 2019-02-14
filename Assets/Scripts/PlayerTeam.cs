using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerTeam : MonoBehaviour
{
    public List<Goblin> Members;
    private Goblin _leader;
    
    public Goblin Leader
    {
        get
        {
            return _leader;
        }
        set
        {
            if (value != _leader)
            {
                _leader = value;
                _leader.name = "Chief " + _leader.name + NameGenerator.GetSurName();
                PopUpText.ShowText(_leader.name +" is new chief!");
                _leader.Xp += 5;
                PlayerController.UpdateFog();
            }
            value = _leader;
        }
    }

    public bool Fighting;

    internal bool AllHidden()
    {
        return Members.All(g => g.Hiding());
    }

    public TextMeshProUGUI TreasureText;
    public TextMeshProUGUI FoodText;
    //TODO: create triggers for when these change, so they can be highlighted for a while
    public int Treasure = 0;
    public int Food = 10;

    public Goblin Challenger;
    private bool updatedListeners;

    //TODO: should be related to distance of travel
    public float RandomMoveFactor = 2f;

    public CampfireObject Campfire;
    public CampfireObject CampfirePrefab;
    

    // Use this for initialization
    public void Initialize (List<Goblin> members)
    {

        Members = members;
        
        if (Members.Count == 0) Members = GetComponentsInChildren<Goblin>().ToList();

        foreach (var character in Members)
        {
            character.Team = this;

            if (character.OnDeath == null) continue;
            character.OnDeath.AddListener(MemberDied);
        }

        if (!Leader)
            Leader = Members.First();

    }
    
    protected void FixedUpdate()
    {
        TreasureText.text = "Goblin TreasurES: " + Treasure;
        FoodText.text = "FOod: " + Food;

        if (GameManager.Instance.GameStarted && Leader.InArea.AnyEnemies() &&
            Members.Any(g => g.InArea == Leader.InArea && g.Attacking()))
        {
            if(!Fighting)
                SoundController.ChangeMusic(SoundBank.Music.Battle);
            Fighting = true;
        }
        else
        {
            if(Fighting)
                SoundController.ChangeMusic(SoundBank.Music.Explore);

            Fighting = false;
        }
    }

    

    #region Orders
    

    public void Move(Area a )
    {
        //if(a && a== Leader.InArea) //if already there
        //    return;
        //var leaderPos = Leader.transform.position;
        //targetPos = target;
        
        foreach (var gobbo in Members)
        {
            //if (gobbo.InArea != Leader.InArea)
            //    continue;

            gobbo.MoveTo(a, Leader == gobbo);
            //Debug.Log(gobbo +" going to " + gobbo.Target);
            
        }
    }
    
    public void Hide()
    {
        foreach (var ch in Members)
        {
            if (!ch.Attacking() && !ch.Fleeing() &&ch.InArea == Leader.InArea)
            {
                ch.Hide();
            }

            StartCoroutine(PlayHideSound(2.5f));
        }
    }

    private IEnumerator PlayHideSound(float wait)
    {
        yield return new WaitForSeconds(wait);

        if(AllHidden())
            SoundController.PlayStinger(SoundBank.Stinger.Sneaking);
    }

    public void Camp()
    {
        if (Food < 2*Members.Count || Leader.InArea.AnyEnemies() || Leader.InArea.PointOfInterest ||Leader.Fleeing())
            return; //TODO: add message for this. Maybe just change camping icon

        //Create camping Gameobject gameobject 
        Campfire = Instantiate(CampfirePrefab);

        //TODO should not just be in the middle, and someone should set it up.
        Campfire.transform.position = Leader.InArea.transform.position;

        Campfire.Team = this;

        Campfire.SetupCamp(10);

        foreach (var gobbo in Members)
        {
            gobbo.ChangeState(Character.CharacterState.Resting);
        }
        
    }

    internal void Attack()
    {
        if (AllHidden())
        {
            foreach (var character in Leader.InArea.PresentCharacters.Where(e => e.tag == "Enemy"))
            {
                character.ChangeState(Character.CharacterState.Surprised, true);
                character.SurprisedStartTime = Time.time;
            }
        }

        foreach (var gobbo in Members)
        {
            if (gobbo.Fleeing() || !gobbo.InArea == Leader.InArea)
                continue;
            if (gobbo.Hiding())
                gobbo.Hidingplace = null;
            
            gobbo.ChangeState(Character.CharacterState.Attacking, AllHidden() ||gobbo == Leader);
        }
    }

    public void Attack(Character character)
    {
        foreach (var gobbo in Members)
        {
            if (gobbo.Fleeing())
                return;
            if (gobbo.Hiding())
                gobbo.Hidingplace = null;

            AttackCharacter(gobbo,character);
        }
    }

    private void AttackCharacter(Goblin gobbo, Character target)
    {

        gobbo.ChangeState(Character.CharacterState.Attacking);
        gobbo.AttackTarget = target;
    }

    public void ChallengeForLeadership(Goblin challenger)
    {
        if (Leader == challenger)
        {
            Debug.LogWarning("Leader cannot challenge themself");
            return;
        }

        Debug.Log("Challenging");

        Challenger = challenger;
        StartCoroutine(ChallengeRoutine());
    }

    private IEnumerator ChallengeRoutine()
    {
        //maybe move camera

        var fightPos = Leader.transform.position;

        //while (Challenger && Challenger.Alive() && Leader.Alive())
        

        Debug.Log("Setting up options");

        var watchDistance = 3;


        //Set up in ring
        foreach (var goblin in Members)
        {
            goblin.ChangeState(Character.CharacterState.Watching);

            if (goblin == Leader || goblin == Challenger)
            {
                //stand and look tough

                goblin.navMeshAgent.SetDestination(fightPos);

            }
            else
            {
                Vector3 rndDirection =  Random.onUnitSphere.normalized * watchDistance;

                rndDirection.y = 0;

                goblin.navMeshAgent.SetDestination(fightPos + rndDirection);
            }
        }

        //CreateChoice

        PlayerChoice.ChoiceOption o1 = new PlayerChoice.ChoiceOption(){ Description = "Let them Fight!",Action = ()=>AttackCharacter(Challenger,Leader)};

        //TODO: other word than support maybe?
        PlayerChoice.ChoiceOption o2 = new PlayerChoice.ChoiceOption() { Description = "Support " + Leader.name, Action = () => Attack(Challenger) };
            
        PlayerChoice.ChoiceOption o3 = new PlayerChoice.ChoiceOption() { Description = "Support " + Challenger.name, Action = () => Attack(Leader) };

        PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] {o1, o2, o3},
            Challenger.name + " challenge " + Leader.name + " to be new chief!");//.\n\nDo you want to choose a chief or let them fight for it?");

        yield return new WaitUntil(()=>!Challenger.Alive() ||!Leader.Alive());

        if (Challenger.Alive()) Leader = Challenger;


    }

    internal void LeaderShout(PlayerController.OrderType shout)
    {
        Leader.Shout(shout.Speech,shout.GoblinSound);
    }

    #endregion

    //TODO: check if this is actually called
    private void MemberDied(Character c)
    {
        var g = c as Goblin;

        if(!g)
            return;

        Members.Remove(g);

        if (c.InArea.AnyEnemies())
            Members.ForEach(m => m.Morale -= m.InArea == c.InArea ? Random.Range(1, (int)(m.MoralLossOnFriendDeath * m.MoralLossModifier)): 0);

        if (Leader == g)
        {
            SelectLeader();
        }
        //PopUpText.ShowText( g.name + " died");
    }


    
    internal void AddXp(int v)
    {
        foreach (var g in Members)
        {
            if (g )
            {
                g.Xp += v;
            }
            else return;
        }
    }

    //TODO: make better
    void SelectLeader()
    {
        var oldLeader = Leader;

        if (Challenger)
        {
            Leader = Challenger;
            Challenger = null;
            return;
        }

        //Try from same area:
        var potentialLeaders = Members.Where(m => m.InArea == Leader.InArea && m.Alive()).ToList();

        if(potentialLeaders.Any())
            //TODO: check that this works. IT DOES NOT
            Leader = potentialLeaders.OrderBy(g=> g.CurrentLevel).First();
        else
        {
            potentialLeaders = Members.Where(m => m.Alive()).ToList();

            if (potentialLeaders.Any())
                Leader = potentialLeaders.OrderBy(g => g.CurrentLevel).First();
            else
                GameManager.GameOver();
        }

        if (oldLeader.InArea != Leader.InArea)
        {
            oldLeader.InArea.RemoveFogOfWar(false);
        }
    }

    internal void Flee()
    {
        //Debug.Log("Fleeing now");
        foreach (var gobbo in Members)
        {
            gobbo.ChangeState(Character.CharacterState.Fleeing, gobbo == Leader);
        }
    }

    public void EquipmentFound(Equipment equipment, Character finder)
    {
        List<PlayerChoice.ChoiceOption> options = new List<PlayerChoice.ChoiceOption>();

        //TODO check for usability

        options.Add(new PlayerChoice.ChoiceOption() {Action = () => finder.Equip(equipment),Description = finder.name});

        if(finder != Leader)
            options.Add(new PlayerChoice.ChoiceOption() { Action = () => Leader.Equip(equipment), Description = Leader.name });

        var pot = Members.Where(g=> g.ClassType != Goblin.Class.Slave && g != finder && g != Leader).OrderByDescending(g => g.Xp);

        if(pot.Any())
            options.Add(new PlayerChoice.ChoiceOption() { Action = () => pot.First().Equip(equipment), Description = pot.First().name });


        PlayerChoice.SetupPlayerChoice(options.ToArray(),"Who gets to keep the " + equipment.name + "?");
    }
}
