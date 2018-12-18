using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MonoBehaviour
{
    public StatEntry XpTextEntry;
    public StatEntry HealthTextEntry;
    public StatEntry RightTextEntry;
    public Text Name;
    public Text ClassLevelText;
    public Image ClassIcon;
    public GameObject ViewHolder;
    private static CharacterView Instance;
    private Character character;
    private readonly List<GameObject> generatedObjects = new List<GameObject>(10);

    void Start()
    {
        if (!Instance) Instance = this;
    }

    public static void ShowCharacter(Goblin c)
    {
        if (!c)
            return;

        Instance.showCharacter(c);
    }

    private void showCharacter(Goblin c)
    {
        ViewHolder.SetActive(true);

        Name.text = c.name;
        var lvl = Goblin.GetLevel((int) c.Xp);
        ClassLevelText.text = "Level " + lvl + " " + ClassName(c.ClassType);
        XpTextEntry.Value.text = c.Xp.ToString("F0") + "/" + Goblin.LevelCaps[lvl];
        HealthTextEntry.Value.text = c.Health + "/" +c.HEA.GetStatMax();
        ClassIcon.sprite = GameManager.GetClassImage(c.ClassType);

        foreach (var stat in c.Stats)
        {
            var entry = Instantiate(RightTextEntry,RightTextEntry.transform.parent);
            entry.Name.text = stat.Name;
            entry.Value.text = stat.GetStatMax().ToString();
            entry.gameObject.SetActive(true);
            generatedObjects.Add(entry.gameObject);
            //TODO: create level up which happens when pressing level up button

            if (c.WaitingOnLevelUp)
            {
                entry.LevelUpStat.onClick.AddListener(() => LevelUp(c, stat));
                entry.LevelUpStat.enabled = true;

            }
            else entry.LevelUpStat.enabled = false;

        }
    }

    private void LevelUp(Goblin gob, Character.Stat stat)
    {
        
        foreach (var ob in generatedObjects)
        {
            ob.GetComponent<StatEntry>().LevelUpStat.enabled = false;
        }

        //TODO: Handle levelup
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

    public void Close()
    {
        foreach (var generatedObject in generatedObjects)
        {
            Destroy(generatedObject);
        }
        generatedObjects.Clear();

        ViewHolder.SetActive(false);
    }

    
}
