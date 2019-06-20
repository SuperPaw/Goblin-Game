using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoblinWarrens : PointOfInterest
{
    public List<Goblin> Members = new List<Goblin>();
    public PlayerChoice.ChoiceOption BuyFood = new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.BuyFood(5, 2), Description = "Ok" };

    public override void SetupMenuOptions()
    {

        PoiOptionController.CreateOption(GameManager.OptionType.BuyGoblin, BuyGoblinBox);
        PoiOptionController.CreateOption(GameManager.OptionType.SellGoblin, SellGoblinBox);
        PoiOptionController.CreateOption(GameManager.OptionType.BuyFood, BuyFoodBox);
    }

    void BuyGoblinBox()
    {
        if (Members.Count == 0)
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "No goblins for sale here.");
        }
        if (team.Treasure >= 5)
        {
            var options = Members.Take(4).Select(g =>
                new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.BuyGoblin(g, 5, this), Description = g.name + " the " + g.ClassType }).ToList();
            options.Add(No);

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                "Buy a Goblin for 5 goblin treasures?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough goblin treasure to buy goblins.");
        }

    }

    void SellGoblinBox()
    {
        //TODO: maybe shuffle first

        var options = team.Members.Where(g => g != team.Leader).Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SellGoblin(g, 2, this), Description = g.name + " the " + g.ClassType }).ToList();
        options.Add(No);

        PlayerChoice.SetupPlayerChoice(options.ToArray(),
            "Sell a Goblin for 2 goblin treasures?");
    }


    void BuyFoodBox()
    {
        if (team.Treasure >= 2)
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { BuyFood, No },
                "Buy 5 food for 2 Goblin Treasures?");
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough goblin treasure to buy food.");
        }

    }

}
