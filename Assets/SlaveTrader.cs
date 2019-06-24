using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlaveTrader : PointOfInterest
{

    public override void SetupMenuOptions()
    {

        PoiOptionController.CreateOption(GameManager.OptionType.BuyGoblin, BuyGoblinBox);
        PoiOptionController.CreateOption(GameManager.OptionType.SellGoblin, SellGoblinBox);
        //attack option
    }


    void BuyGoblinBox()
    {
        if (team.Treasure >= 6)
        {
            var options = new List<PlayerChoice.ChoiceOption>
            {
                OkOption,
                No
            };

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                "Buy a Goblin Slave for 6 goblin treasures?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough goblin treasure to buy slaves.");
        }

    }

    void SellGoblinBox()
    {
        //TODO: maybe shuffle first

        var options = team.Members.Where(g => g != team.Leader).Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SellGoblin(g, 2), Description = g.name + " the " + g.ClassType }).ToList();
        options.Add(No);

        PlayerChoice.SetupPlayerChoice(options.ToArray(),
            "Sell a Goblin for 2 goblin treasures?");
    }
}
