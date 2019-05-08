using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpView : MenuWindow
{
    public Goblin Character;
    //public GameObject LevelUpViewHolder;
    public LevelController.LevelUpChoice SelectedChoice;
    public TextMeshProUGUI ChoiceExplanantion;
    private List<Button> generatedClassButtons = new List<Button>();
    [SerializeField] private Button LevelUpClassIcon;


    new void Awake()
    {
        base.Awake();

        Type = WindowType.LevelUp;
    }
    public void SetupLevelScreen(Goblin character)
    {
        if(!character) return;
        
        foreach (var generatedClassButton in generatedClassButtons)
        {
            Destroy(generatedClassButton.gameObject);
        }
        generatedClassButtons.Clear();

        ViewHolder.SetActive(true);

        Character = character;

        if (character.WaitingOnClassSelection)
        {

            if (generatedClassButtons == null || generatedClassButtons.Count == 0)
            {
                generatedClassButtons = new List<Button>();

                for (Goblin.Class i = (Goblin.Class)2; i < Goblin.Class.END; i = (Goblin.Class)((int)i * 2))
                {
                    var clBut = Instantiate(LevelUpClassIcon, LevelUpClassIcon.transform.parent);

                    clBut.image.sprite = GameManager.GetClassImage(i);

                    var cl = i;

                    clBut.GetComponent<OnValueHover>().Class = i;

                    generatedClassButtons.Add(clBut);

                    clBut.onClick.AddListener(() => SelectClass(character, cl));

                    clBut.gameObject.SetActive(true);
                }
            }
            //ClassSelectText.text = "Select Class:";
            LevelUpClassIcon.gameObject.SetActive(false);
        }
        else
        {
            LevelUpClassIcon.gameObject.SetActive(true);
            //ClassSelectText.text = "";
        }
    }

    public void SelectChoice(LevelController.LevelUpChoice c)
    {
        SelectedChoice = c;
    }

    public void ConfirmChoice()
    {
        Debug.Log("Selection confirmed");

        switch (SelectedChoice.Type)
        {
            case LevelController.ChoiceType.Attribute:
                break;
            case LevelController.ChoiceType.Class:
                break;
            case LevelController.ChoiceType.Skill:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ViewHolder.SetActive(false);
    }


    private void SelectClass(Goblin c, Goblin.Class cl)
    {
        c.SelectClass(cl);
        Close();

        //TODO: use goblin change event
        GoblinUIList.UpdateGoblinList();

        //TODO: use a character stat change event instead.
        CharacterView.ShowCharacter(c);
    }



    //TODO: move to character or game manager
    private void LevelUp(Goblin gob, Character.Stat stat)
    {

        stat.LevelUp();

        gob.WaitingOnLevelUp--;
        //if (gob.WaitingOnLevelUp < 1)
        //    foreach (var ob in generatedObjects)
        //    {
        //        var s = ob.GetComponent<StatEntry>();
        //        if (s)
        //        {
        //            s.LevelUpStat.gameObject.SetActive(false);
        //            if (s.Name.text == stat.Type.ToString())
        //                s.Value.text = stat.GetStatMax().ToString();
        //        }
        //    }

        GoblinUIList.UpdateGoblinList();
    }
}
