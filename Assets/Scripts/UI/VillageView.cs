using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VillageView : MenuWindow
{
    public static VillageView Instance;
    public GoblinWarrens ShowingVillage;
    private PlayerTeam team;
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() {Action = null,Description = "No"};
    public PlayerChoice.ChoiceOption BuyFood = new PlayerChoice.ChoiceOption() { Action = ()=> PlayerController.BuyFood(5,2), Description = "Ok" };
    

    public Button BuyFoodButton,
        BuyGobboButton,
        SellGobButton,
        DirectionsButton,
        RestButton,
        GoblinGameButton,
        ChallengeChiefButton;

    new void Awake()
    {
        base.Awake();

        Type = WindowType.LocationView;

        if (!Instance)
            Instance = this;

        BuyFoodButton.onClick.AddListener(BuyFoodBox);

        BuyGobboButton.onClick.AddListener(BuyGoblinBox);

        SellGobButton.onClick.AddListener(SellGoblinBox);

    }
    
    public static void OpenVillageView(GoblinWarrens village, PlayerTeam playerTeam)
    {
        Instance.OpenWindow(village,playerTeam);
    }


    private void OpenWindow(GoblinWarrens village, PlayerTeam playerTeam)
    {
        ShowingVillage = village;

        team = playerTeam;
        
        Open();
    }


    void BuyGoblinBox()
    {
        if (ShowingVillage.Members.Count == 0)
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "No goblins for sale here.");
        }
        if (team.Treasure >= 5)
        {
            var options = ShowingVillage.Members.Take(4).Select(g =>
                new PlayerChoice.ChoiceOption() {Action = () => PlayerController.BuyGoblin(g, 5,ShowingVillage), Description = g.name + " the "+ g.ClassType}).ToList();
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

        var options = team.Members.Where(g=>g != team.Leader).Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerController.SellGoblin(g, 2,ShowingVillage), Description = g.name + " the " + g.ClassType }).ToList();
        options.Add(No);

        PlayerChoice.SetupPlayerChoice(options.ToArray(),
            "Sell a Goblin for 2 goblin treasures?");
    }


    void BuyFoodBox()
    {
        if (team.Treasure >= 2)
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] {BuyFood, No},
                "Buy 5 food for 2 Goblin Treasures?");
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have enough goblin treasure to buy food.");
        }

    }

    public new void Close()
    {
        base.Close();
    }
}
