using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cave : PointOfInterest
{
    public int Difficulty;
    public int Prize;
    public bool Explored;

    public void LureMonster(PlayerTeam team, int food)
    {
        team.OnFoodFound.Invoke(-food);

        StartCoroutine(Spawning(team));
    }


    public void SendInGoblin(Goblin g)
    {
        StartCoroutine(SendGoblinInCaveRoutine(g));
    }

    private IEnumerator SendGoblinInCaveRoutine(Goblin g)
    {
        //Goblin walk there
        g.MoveTo(transform.position);

        //Wait for resolution
        yield return new WaitForSeconds(2);
        
        //turn off goblin
        g.gameObject.SetActive(false);

        //Wait for resolution
        yield return new WaitForSeconds(2);

        //Arrive back with treasure or Remove goblin
        if (Random.Range(0, g.SMA.GetStatMax()) >= Difficulty)
        {
            g.Xp += 10;
            g.Team.OnTreasureFound.Invoke(Prize);
            PopUpText.ShowText(g.name + " found many goblin treasures in Cave!");
            g.gameObject.SetActive(true);
            g.ChangeState(Character.CharacterState.Idling,true);

            Explored = true;
        }
        else
        {
            PopUpText.ShowText(g.name + " did not return from exploring the cave!");
            g.Health = 0;
            g.Team.Members.Remove(g);
        }
    }

    public override void SetupMenuOptions()
    {
        PoiOptionController.CreateOption(GameManager.OptionType.Lure, LureMonsterBox);
        PoiOptionController.CreateOption(GameManager.OptionType.Explore, SendInGoblinBox);

    }

    void SendInGoblinBox()
    {
        //TODO: maybe shuffle first
        if (!Explored)
        {
            var options = team.Members.Where(ge => ge.InArea == InArea).Take(4).Select(g =>
                new PlayerChoice.ChoiceOption()
                {
                    Action = () => SendInGoblin(g),
                    Description = g.name + " the " + g.ClassType
                }).ToList();
            options.Add(No);

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                "Send a Goblin in to the Cave?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You have already explored the cave.");
        }
    }


    void LureMonsterBox()
    {
        if (team.Food >= 1)
        {
            var amount = Mathf.Min(team.Food, 3);

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => LureMonster(team,amount), Description = "Ok" },
                    No
                },
                "Put " + amount.ToString("D") + " food out to attract monster?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have any food put out.");
        }
    }
}
