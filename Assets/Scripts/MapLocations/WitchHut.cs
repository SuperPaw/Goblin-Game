using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitchHut : PointOfInterest
{

    public void Attack(PlayerTeam team)
    {
        StartCoroutine(Spawning(team));
    }

    public void PayToHeal(int i, PlayerTeam team)
    {
        team.Members.ForEach(g=>g.Heal());
        team.OnTreasureFound.Invoke(-i);
        
    }

    public void BuyStaff(int amount, PlayerTeam team)
    {
        team.OnEquipmentFound.Invoke(EquipmentGen.GetEquipment(Equipment.EquipmentType.Stick),team.Leader);

        team.OnTreasureFound.Invoke(- amount);
    }

    public void BuySkull(int amount, PlayerTeam team)
    {
        team.OnEquipmentFound.Invoke(EquipmentGen.GetEquipment(Equipment.EquipmentType.Skull), team.Leader);

        team.OnTreasureFound.Invoke(-amount);
    }


    public override void SetupMenuOptions()
    {
        PoiOptionController.CreateOption(PointOfInterest.OptionType.Healing, Heal);
        PoiOptionController.CreateOption(PointOfInterest.OptionType.BuyStaff, BuyStaff);
        PoiOptionController.CreateOption(PointOfInterest.OptionType.BuyHat, BuyHat);
        PoiOptionController.CreateOption(PointOfInterest.OptionType.Attack, () =>
            PlayerChoice.CreateDoChoice(() => Attack(team), "Do you want to attack the witch"));

    }

    #region Helpers


    private void Heal()
    {
        if (team.Treasure >= 2)
        {

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => PayToHeal(2,team), Description = "Ok" },
                    No
                },
                "Heal goblins for 2 treasure?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough treasure to pay the witch.");
        }
    }
    
    private void BuyStaff()
    {
        var amount = 5;

        if (team.Treasure >= amount)
        {

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => BuyStaff(amount,team), Description = "Ok" },
                    No
                },
                "Buy magic stick for 5 treasure?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough treasure to buy stick.");
        }
    }

    private void BuyHat()
    {
        var amount = 5;

        if (team.Treasure >= amount)
        {

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => BuySkull(amount,team), Description = "Ok" },
                    No
                },
                "Buy Skull hat for 5 treasure?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough treasure to buy skull.");
        }
    }
#endregion
}
