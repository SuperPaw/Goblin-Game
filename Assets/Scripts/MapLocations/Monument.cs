using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monument : PointOfInterest
{
    public Character ZombiePrefab;
    public int Treasure = 3;

    public PlayerChoice.ChoiceOption SacTreasureOption = new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SacTreasure(3), Description = "Ok" };


    public void SpawnDead(PlayerTeam team)
    {
        StartCoroutine(Spawning(team));
    }

    private new IEnumerator Spawning(PlayerTeam team)
    {
        yield return new WaitForSeconds(1.5f);

        Sun.Night();

        var zs = Random.Range(5, 10);

        for (int i = 0; i < zs; i++)
        {
            var z = MapGenerator.GenerateCharacter(ZombiePrefab.gameObject, InArea, NpcHolder.Instance.transform,true).GetComponent<Character>();

            z.ChangeState(Character.CharacterState.Attacking);

            team.Members.ForEach(m=> m.Morale -= 1);

            yield return new WaitForSeconds(Random.Range(0,1.5f));
        }
    }
    
    public override void SetupMenuOptions()
    {
        PoiOptionController.CreateOption(PointOfInterest.OptionType.SacrificeGoblin, SacGoblinBox);
    }


    void SacTreasureBox()
    {
        if (team.Treasure >= 3)
        {
            var options = new List<PlayerChoice.ChoiceOption>
            {
                SacTreasureOption,
                No
            };

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                "Sacrifice 3 goblin treasure to Big Stone?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough goblin treasure to sacrifice.");
        }

    }
    
    void SacGoblinBox()
    {
        //TODO: maybe shuffle first

        var options = team.Members.Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SacGoblin(g, this), Description = g.name + " the " + g.ClassType }).ToList();
        options.Add(No);

        PlayerChoice.SetupPlayerChoice(options.ToArray(),
            "Sacrifice a Goblin to the Big stone?");
    }


    void SacFoodBox()
    {
        if (team.Food >= 1)
        {
            var amount = Mathf.Min(team.Food, 5);

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SacFood(amount), Description = "Ok" },
                    No
                },
                "Sacrifice " + amount.ToString("D") + " food to Big Stone?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have any food to sacrifice.");
        }
    }
}
