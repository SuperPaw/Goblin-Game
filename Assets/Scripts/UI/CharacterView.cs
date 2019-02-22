using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MenuWindow
{
    public StatEntry XpTextEntry;
    public StatEntry HealthTextEntry;
    public StatEntry RightTextEntry;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI ClassLevelText;
    public TextMeshProUGUI EquipmentInfo;
    public TextMeshProUGUI ClassSelectText;
    public GameObject ClassSelectionHolder;
    public Button ClassIcon;
    private static CharacterView Instance;
    private Goblin character;
    private readonly List<GameObject> generatedObjects = new List<GameObject>(10);
    private List<Button> generatedClassButtons = new List<Button>();

    new void Awake()
    {
        base.Awake();

        Type = WindowType.Character;

        if (!Instance) Instance = this;
    }

    public static void ShowCharacter(Goblin c)
    {
        if (!c)
            return;

        Instance.showCharacter(c);
    }

    public static void AddXp()
    {
        if(!Instance.ViewHolder.activeInHierarchy || !Instance.character)
            return;

        Instance.character.Xp += 10;
        Instance.showCharacter(Instance.character);
    }

    private void showCharacter(Goblin c)
    {
        Open();

        character = c;

        PlayerController.FollowGoblin = c;

        foreach (var generatedObject in generatedObjects)
        {
            Destroy(generatedObject);
        }
        generatedObjects.Clear();

        foreach (var generatedClassButton in generatedClassButtons)
        {
            Destroy(generatedClassButton.gameObject);
        }
        generatedClassButtons.Clear();


        Name.text = c.name;
        var lvl = Goblin.GetLevel((int) c.Xp);
        ClassLevelText.text = "Level " + lvl + " " + ClassName(c.ClassType);
        ClassLevelText.GetComponent<OnValueHover>().Class = c.ClassType;
        ClassLevelText.gameObject.SetActive(!c.WaitingOnClassSelection);
        ClassSelectionHolder.SetActive(c.WaitingOnClassSelection);


        XpTextEntry.Value.text = c.Xp.ToString("F0") + "/" + Goblin.LevelCaps[lvl];
        HealthTextEntry.Value.text = c.Health + "/" +c.HEA.GetStatMax();
        HealthTextEntry.ValueHover.Stat = c.HEA;
        ClassIcon.image.sprite = GameManager.GetClassImage(c.ClassType);
        ClassIcon.GetComponent<OnValueHover>().Class = c.ClassType;


        //TODO: update current stats instead replacing
        foreach (var stat in c.Stats.Values)
        {
            var entry = Instantiate(RightTextEntry, RightTextEntry.transform.parent);
            entry.Name.text = stat.Type.ToString();
            entry.Value.text = stat.GetStatMax().ToString();
            entry.gameObject.SetActive(true);
            entry.ValueHover.Stat = stat;
            generatedObjects.Add(entry.gameObject);
            //TODO: create level up which happens when pressing level up button

            if (c.WaitingOnLevelUp > 0)
            {
                entry.LevelUpStat.onClick.AddListener(() => LevelUp(c, stat));
                entry.LevelUpStat.gameObject.SetActive(true);

            }
            else entry.LevelUpStat.gameObject.SetActive(false);
        }

        //TODO: update current stats instead replacing
        foreach (var equipment in c.Equipped.Values.Where(v => v))
        {
            var entry = Instantiate(EquipmentInfo, EquipmentInfo.transform.parent);
            entry.text = equipment.EquipLocation + ": " + equipment.name;
            entry.gameObject.SetActive(true);
            var hover =entry.GetComponent<OnValueHover>();
            if (hover)
            {
                hover.ShowEquipment = true;
                hover.Equipment = equipment;
            }

            generatedObjects.Add(entry.gameObject);
        }

        if (c.WaitingOnClassSelection)
        {

            if (generatedClassButtons == null || generatedClassButtons.Count == 0)
            {
                generatedClassButtons = new List<Button>();

                for (Goblin.Class i = (Goblin.Class)1; i < Goblin.Class.END; i++)
                {
                    var clBut = Instantiate(ClassIcon,ClassIcon.transform.parent);

                    clBut.image.sprite = GameManager.GetClassImage(i);

                    var cl = i;

                    clBut.GetComponent<OnValueHover>().Class = i;
                        
                    generatedClassButtons.Add(clBut);

                    clBut.onClick.AddListener(() =>SelectClass(c,cl));

                    clBut.gameObject.SetActive(true);
                }
            }
            ClassSelectText.text = "Select Class:";
            ClassIcon.gameObject.SetActive(false);
        }
        else
        {
            ClassIcon.gameObject.SetActive(true);
            ClassSelectText.text = "";
        }

    }

    private void SelectClass(Goblin c, Goblin.Class cl)
    {
        c.SelectClass(cl);
        Close();

        GoblinUIList.UpdateGoblinList();

        showCharacter(c);
    }

    //TODO: move to character or game manager
    private void LevelUp(Goblin gob, Character.Stat stat)
    {

        stat.LevelUp();

        gob.WaitingOnLevelUp--;
        if(gob.WaitingOnLevelUp < 1)
            foreach (var ob in generatedObjects)
            {
                var s =ob.GetComponent<StatEntry>();
                if (s)
                {
                    s.LevelUpStat.gameObject.SetActive(false);
                    if (s.Name.text == stat.Type.ToString())
                        s.Value.text = stat.GetStatMax().ToString();
                }
            }
        
        GoblinUIList.UpdateGoblinList();
    }

    private string ClassName(Goblin.Class cl)
    {
        switch (cl)
        {
            case Goblin.Class.Swarmer:
                return "Goblin Swarmer";
            case Goblin.Class.Shooter:
                return "Goblin Shooter";
            case Goblin.Class.Ambusher:
                return "Goblin Ambusher";
            case Goblin.Class.Scout:
                return "Goblin Scout";
            case Goblin.Class.Slave:
                return "Goblin Slave";
            default:
                return "Goblin";
        }
    }
    

    
}
