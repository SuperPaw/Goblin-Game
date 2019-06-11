using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CaveView : MenuWindow
{

    public static CaveView Instance;
    public Cave Cave;
    private PlayerTeam team;
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() { Action = null, Description = "No" };

    public Button LureMonsterButton,
        SendInGoblinButton;

    new void Awake()
    {
        base.Awake();

        Type = WindowType.LocationView;

        if (!Instance)
            Instance = this;

        LureMonsterButton.onClick.AddListener(LureMonsterBox);

        SendInGoblinButton.onClick.AddListener(SendInGoblinBox);
    }

    public static void OpenCaveView(Cave c, PlayerTeam playerTeam)
    {
        Instance.OpenWindow(c, playerTeam);
    }

    public static void CloseCave()
    {
        Instance.Close();
    }

    private void OpenWindow(Cave cave, PlayerTeam playerTeam)
    {
        Cave = cave;

        team = playerTeam;

        var poiControl = cave.GetComponentInChildren<PoiOptionController>();

        poiControl.CreateOption(PoiOptionController.OptionType.Lure,PoiOptionController.OptionTarget.Monster, LureMonsterBox);
        poiControl.CreateOption(PoiOptionController.OptionType.Explore, PoiOptionController.OptionTarget.Cave, SendInGoblinBox);

        //Open();
    }


    void SendInGoblinBox()
    {
        //TODO: maybe shuffle first
        if (!Cave.Explored)
        {
            var options = team.Members.Where(ge=> ge.InArea == Cave.InArea).Take(4).Select(g =>
                new PlayerChoice.ChoiceOption()
                {
                    Action = () => Cave.SendInGoblin(g),
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
                    new PlayerChoice.ChoiceOption() { Action = () => Cave.LureMonster(team,amount), Description = "Ok" },
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
