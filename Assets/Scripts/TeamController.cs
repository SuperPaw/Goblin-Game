using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TeamController : MonoBehaviour
{
    public List<Goblin> Members;
    private Goblin leader;
    
    public Goblin Leader
    {
        get
        {
            return leader;
        }
        set
        {
            if (value != leader)
            {
                leader = value;
                leader.name = "Chief " + leader.name + NameGenerator.GetSurName();
            }
            value = leader;
        }
    }

    public TextMeshProUGUI TreasureText;
    public TextMeshProUGUI FoodText;
    //TODO: create triggers for when these change, so they can be highlighted for a while
    public int Treasure = 0;
    public int Food = 10;

    public Goblin Challenger;
    private Coroutine _challengeRoutine;
    private bool updatedListeners;

    //TODO: should be related to distance of travel
    public float RandomMoveFactor = 2f;

    private Vector3 targetPos;

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

    private void Update()
    {
        Debug.DrawLine(targetPos, targetPos + Vector3.up);

    }

    protected void FixedUpdate()
    {
        TreasureText.text = "Goblin TreasurES: " + Treasure;
        FoodText.text = "FOod: " + Food;
    }


    #region Orders

    public void Move(Vector3 target,Area a = null)
    {
        //if(a && a== Leader.InArea) //if already there
        //    return;
        //var leaderPos = Leader.transform.position;
        //targetPos = target;
        
        foreach (var gobbo in Members)
        {
            gobbo.ChangeState(Character.CharacterState.Travelling);

            gobbo.Target = a ? a.GetRandomPosInArea() : target;

            gobbo.actionInProgress = true;
            Debug.Log(gobbo +" going to " + gobbo.Target);
            
        }
    }
    
    public void Hide()
    {
        foreach (var ch in Members)
        {
            if (!ch.Attacking() && !ch.Fleeing())
            {
                ch.Hide();
            }
        }
    }

    internal void Attack()
    {
        foreach (var gobbo in Members)
        {
            if (gobbo.Fleeing())
                return;
            if (gobbo.Hiding())
                gobbo.Hidingplace = null;

            gobbo.ChangeState(Character.CharacterState.Attacking);
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
        _challengeRoutine = StartCoroutine(ChallengeRoutine());
    }

    private IEnumerator ChallengeRoutine()
    {
        //maybe move camera

        var fightPos = leader.transform.position;

        //while (Challenger && Challenger.Alive() && Leader.Alive())
        

        Debug.Log("Setting up options");

        var watchDistance = 3;


        //Set up in ring
        foreach (var goblin in Members)
        {
            goblin.ChangeState(Character.CharacterState.Watching);

            if (goblin == leader || goblin == Challenger)
            {
                //stand and look tough

                goblin.Target = fightPos;

            }
            else
            {
                Vector3 rndDirection = new Vector3(Random.Range(-1,1),0, Random.Range(-1, 1)).normalized * watchDistance;

                goblin.Target = fightPos + rndDirection;
            }
        }

        //CreateChoice

        PlayerChoice.ChoiceOption o1 = new PlayerChoice.ChoiceOption(){ Description = "Fight!",Action = ()=>AttackCharacter(Challenger,leader)};

        PlayerChoice.ChoiceOption o2 = new PlayerChoice.ChoiceOption() { Description = "Choose " + leader.name, Action = () => Attack(Challenger) };
            
        PlayerChoice.ChoiceOption o3 = new PlayerChoice.ChoiceOption() { Description = "Choose " + Challenger.name, Action = () => Attack(leader) };

        PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] {o1, o2, o3}, Challenger.name + " has challenged "+ leader.name+ ".\n\nDo you want to choose a chief or let them fight for it?");

        yield return new WaitUntil(()=>!Challenger.Alive() ||!Leader.Alive());

        if (Challenger.Alive()) Leader = Challenger;


    }

    #endregion

    //TODO: check if this is actually called
    private void MemberDied(Character c)
    {
        var g = c as Goblin;

        if(!g)
            return;

        Members.Remove(g);

        if (Leader == g)
        {
            SelectLeader();
        }
        Debug.Log("Team member : "+ g.name + " died");

        Members.ForEach(m=> m.Moral -= m.MoralLossOnFriendDeath);
    }


    
    internal void AddXp(int v)
    {
        foreach (var g in Members)
        {
            if (g as Goblin)
            {
                (g as Goblin).Xp += v;
            }
            else return;
        }
    }

    //TODO: make better
    void SelectLeader()
    {
        Leader = Members.First(m => m.isActiveAndEnabled);
    }

    internal void Flee()
    {
        foreach (var gobbo in Members)
        {
            gobbo.ChangeState(Character.CharacterState.Fleeing);
        }
    }

}
