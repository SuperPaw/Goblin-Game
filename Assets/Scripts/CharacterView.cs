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
    public Button ClassIcon;
    public Text ClassSelectText;
    public GameObject ViewHolder;
    private static CharacterView Instance;
    private Character character;
    private readonly List<GameObject> generatedObjects = new List<GameObject>(10);
    private List<Button> generatedClassButtons ;

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
        HealthTextEntry.ValueHover.Stat = c.HEA;
        ClassIcon.image.sprite = GameManager.GetClassImage(c.ClassType);


        //TODO: update current stats instead replacing
        foreach (var stat in c.Stats)
        {
            var entry = Instantiate(RightTextEntry,RightTextEntry.transform.parent);
            entry.Name.text = stat.Name;
            entry.Value.text = stat.GetStatMax().ToString();
            entry.gameObject.SetActive(true);
            entry.ValueHover.Stat = stat;
            generatedObjects.Add(entry.gameObject);
            //TODO: create level up which happens when pressing level up button

            if (c.WaitingOnLevelUp)
            {
                entry.LevelUpStat.onClick.AddListener(() => LevelUp(c, stat));
                entry.LevelUpStat.gameObject.SetActive(true);

            }
            else entry.LevelUpStat.gameObject.SetActive(false);

            if (c.WaitingOnClassSelection)
            {
                if (generatedClassButtons == null)
                {
                    generatedClassButtons = new List<Button>();

                    for (Goblin.Class i = (Goblin.Class)1; i < Goblin.Class.END; i++)
                    {
                        var clBut = Instantiate(ClassIcon,ClassIcon.transform.parent);

                        clBut.image.sprite = GameManager.GetClassImage(i);

                        var cl = i;

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
    }

    private void SelectClass(Goblin c, Goblin.Class cl)
    {
        foreach (var generatedClassButton in generatedClassButtons)
        {
            Destroy(generatedClassButton.gameObject);
        }
        generatedClassButtons.Clear();

        c.SelectClass(cl);
        Close();

        showCharacter(c);
    }

    //TODO: move to character or game manager
    private void LevelUp(Goblin gob, Character.Stat stat)
    {

        stat.LevelUp();
        foreach (var ob in generatedObjects)
        {
            var s =ob.GetComponent<StatEntry>();
            s.LevelUpStat.gameObject.SetActive(false);
            if(s.Name.text == stat.Name)
                s.Value.text = stat.GetStatMax().ToString();
        }
        
        gob.WaitingOnLevelUp = false;
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
