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
    public StatEntry StatEntry;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI ClassLevelText;
    public TextMeshProUGUI EquipmentInfo;
    //public TextMeshProUGUI ClassSelectText;
    //public GameObject ClassSelectionHolder;
    [SerializeField]
    private Image ClassIcon = null;
    private static CharacterView Instance;
    private Goblin character;
    private readonly List<GameObject> generatedObjects = new List<GameObject>(10);
    [SerializeField]
    private Button levelUpButton = null;
    [SerializeField]
    private LevelUpView levelUpScreen = null;

    new void Awake()
    {
        base.Awake();

        Type = WindowType.Character;

        levelUpButton.onClick.AddListener(OpenLevelUp);

        if (!Instance) Instance = this;
    }

    private void OpenLevelUp()
    {
        levelUpScreen.SetupLevelScreen(character);
    }

    public static void ShowCharacter(Goblin c)
    {
        if (!c || !c.Team)// || c.Team.Leader.InArea != c.InArea)
            return;

        //Debug.Log("Showing character: "+ c);

        Instance.showCharacter(c);
    }

    public static void AddXp()
    {
        if(!Instance.ViewHolder.gameObject.activeInHierarchy || !Instance.character)
            return;

        Instance.character.Xp += 10;
        Instance.showCharacter(Instance.character);
    }

    public static void Kill()
    {
        if (!Instance.ViewHolder.gameObject.activeInHierarchy || !Instance.character)
            return;

        Instance.character.Kill();
    }

    private void showCharacter(Goblin c)
    {
        Open();

        levelUpScreen.Close();

        character = c;

        PlayerController.Follow(c);

        foreach (var generatedObject in generatedObjects)
        {
            Destroy(generatedObject);
        }
        generatedObjects.Clear();
        
        Name.text = c.name;
        var lvl = Goblin.GetLevel((int) c.Xp);
        ClassLevelText.text = "Level " + lvl + " " + ClassName(c.ClassType);
        ClassLevelText.GetComponent<OnValueHover>().Class = c.ClassType;
        //ClassLevelText.gameObject.SetActive(!c.WaitingOnClassSelection || !c.Alive());
        //ClassSelectionHolder.SetActive(c.WaitingOnClassSelection && c.Alive());

        levelUpButton.interactable = c.WaitingOnLevelup();

        if (c.WaitingOnLevelup())//LevelUps > 0 || c.WaitingOnClassSelection)
        {
            XpTextEntry.Value.text = "LVL up";
            XpTextEntry.FillImage.fillAmount = 1;
            levelUpButton.interactable = true;
        }
        else
        {
            XpTextEntry.Value.text = c.Xp.ToString("F0") + "\n - \n" + Goblin.LevelCaps[lvl];
            XpTextEntry.FillImage.fillAmount = c.Xp / Goblin.LevelCaps[lvl];
        }
        HealthTextEntry.Value.text = c.Health + "\n - \n" + c.HEA.GetStatMax();
        HealthTextEntry.FillImage.fillAmount = c.Health / (float) c.HEA.GetStatMax();
        //HealthTextEntry.ValueHover.Stat = c.HEA;
        ClassIcon.sprite = c.Alive() ? GameManager.GetClassImage(c.ClassType): GameManager.GetIconImage(GameManager.Icon.Dead);
        ClassIcon.GetComponent<OnValueHover>().Class = c.ClassType;
        
        //TODO: update current stats instead replacing
        foreach (var stat in c.Stats.Values)
        {
            


            var entry = Instantiate(StatEntry, StatEntry.transform.parent);

            var val = stat.GetStatMax();

            entry.Name.text = stat.Type.ToString();
            entry.Value.text = val.ToString();
            var style = val > 3 ? FontStyles.Bold : FontStyles.Normal;
            var color = val > 3 ? entry.HighStatColor : val < 3 ? entry.LowStatColor : entry.Value.color;
            entry.Value.color = color;
            entry.Name.fontStyle = entry.Value.fontStyle = style;
            entry.gameObject.SetActive(true);
            entry.ValueHover.Stat = stat;
            generatedObjects.Add(entry.gameObject);
            
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


    }


    private string ClassName(Goblin.Class cl)
    {
        switch (cl)
        {
            case Goblin.Class.Meatshield:
                return "Goblin Meatshield";
            case Goblin.Class.Shooter:
                return "Goblin Shooter";
            case Goblin.Class.Ambusher:
                return "Goblin Ambusher";
            case Goblin.Class.Scout:
                return "Goblin Scout";
            case Goblin.Class.Slave:
                return "Goblin Slave";
            case Goblin.Class.Necromancer:
                return "Goblin Necromancer";
            case Goblin.Class.Beastmaster:
                return "Goblin Beastmaster";
            case Goblin.Class.Hunter:
                return "Goblin Hunter";
            case Goblin.Class.Cook:
                return "Goblin Cook";
            case Goblin.Class.Shaman:
                return "Goblin Shaman";
            case Goblin.Class.Diplomat:
                return "Goblin Diplomat";
            case Goblin.Class.NoClass:
            case Goblin.Class.Goblin:
            case Goblin.Class.END:
            case Goblin.Class.ALL:
            default:
                return "Goblin";
        }
    }
    

    
}
