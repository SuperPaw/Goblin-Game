using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WitchHutView : MenuWindow
{

    public static WitchHutView Instance;
    public WitchHut ShowingHut;
    private PlayerTeam team;
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() { Action = null, Description = "No" };
    
    public Button BuyHatButton,
        BuyStaffButton,
        HealGoblinsButton,
        AttackButton;

    new void Awake()
    {
        base.Awake();

        Type = WindowType.LocationView;

        if (!Instance)
            Instance = this;

        BuyHatButton.onClick.AddListener(BuyHat);
        BuyStaffButton.onClick.AddListener(BuyStaff);
        AttackButton.onClick.AddListener(Attack);
        HealGoblinsButton.onClick.AddListener(Heal);
    }


    public static void OpenWitchView(WitchHut w, PlayerTeam playerTeam)
    {
        if(!w.HasBeenAttacked)
            Instance.OpenWindow(w, playerTeam);
    }

    public static void CloseHut()
    {
        Instance.Close();
    }

    #region Helpers


    private void Heal()
    {
        if (team.Treasure >= 2)
        {

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => ShowingHut.PayToHeal(2,team), Description = "Ok" },
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

    private void Attack()
    {
        ShowingHut.Attack(team);
    }

    private void BuyStaff()
    {
        var amount = 5;

        if (team.Treasure >= amount)
        {

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => ShowingHut.BuyStaff(amount,team), Description = "Ok" },
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
                    new PlayerChoice.ChoiceOption() { Action = () => ShowingHut.BuySkull(amount,team), Description = "Ok" },
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


    private void OpenWindow(WitchHut w, PlayerTeam playerTeam)
    {
        ShowingHut = w;

        team = playerTeam;

        Open();
    }

    #endregion
}
