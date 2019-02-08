using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BigStoneView : MenuWindow
{
    public static BigStoneView Instance;
    public Monument Monument;
    private PlayerTeam team;
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() {Action = null,Description = "No"};
    public PlayerChoice.ChoiceOption SacTreasureOption = new PlayerChoice.ChoiceOption() { Action = () => PlayerController.SacTreasure(3), Description = "Ok" };


    public Button SacFoodButton,
        SacTreasureButton,
        SacGobButton,
        StealTreasureButton;

    new void Start()
    {
        base.Start();

        Type = WindowType.LocationView;

        if (!Instance)
            Instance = this;

        SacFoodButton.onClick.AddListener(SacFoodBox);

        SacTreasureButton.onClick.AddListener(SacTreasureBox);

        SacGobButton.onClick.AddListener(SacGoblinBox);
        StealTreasureButton.onClick.AddListener(StealTreasureBox);

    }
    
    public static void OpenStoneView(Monument stone, PlayerTeam playerTeam)
    {
        Instance.OpenWindow(stone,playerTeam);
    }


    private void OpenWindow(Monument stone, PlayerTeam playerTeam)
    {
        Monument = stone;

        team = playerTeam;
        
        Open();
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

    void StealTreasureBox()
    {
        if (Monument.Treasure <= 0)
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "No treasure to steal.");
        }
        else 
        {
            var options = new List<PlayerChoice.ChoiceOption>() { 
                new PlayerChoice.ChoiceOption() { Action = () => PlayerController.StealTreasure(Monument), Description = "Yes" },
                No
            };

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                "Steal treasure from Big Stone?");
        }
    }

    void SacGoblinBox()
    {
        //TODO: maybe shuffle first

        var options = team.Members.Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerController.SacGoblin(g, Monument), Description = g.name + " the " + g.ClassType }).ToList();
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
                    new PlayerChoice.ChoiceOption() { Action = () => PlayerController.SacFood(amount), Description = "Ok" },
                    No
                },
                "Sacrifice " + amount.ToString("D") + " food to Big Stone?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] {OkOption},
                "You do not have any food to sacrifice.");
        }
    }

    public new void Close()
    {
        base.Close();
    }
}
