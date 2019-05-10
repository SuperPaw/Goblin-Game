using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpView : MenuWindow
{
    private Goblin character;
    //public GameObject LevelUpViewHolder;
    public LevelController.LevelUpChoice[] Choices;
    public LevelController.LevelUpChoice SelectedChoice;
    public TextMeshProUGUI ChoiceExplanantion;
    //public TextMeshProUGUI SelectText;
    private List<Button> generatedClassButtons = new List<Button>();
    [SerializeField] private Button LevelUpClassIcon;
    public LevelController.LevelUpChoice[] ClassChoices;
    public Button ConfirmButton;


    new void Awake()
    {
        base.Awake();

        Type = WindowType.LevelUp;
    }
    public void SetupLevelScreen(Goblin character)
    {
        if(!character) return;

        ConfirmButton.interactable = false;
        
        foreach (var generatedClassButton in generatedClassButtons)
        {
            Destroy(generatedClassButton.gameObject);
        }
        generatedClassButtons.Clear();

        ViewHolder.SetActive(true);

        this.character = character;
        
        ChoiceExplanantion.text = character.WaitingOnClassSelection ? "Select a Class": "Select level up";

        //TODO: use progression system instead
        Choices = character.WaitingOnClassSelection ? ClassChoices : LevelController.GetLevelUpChoices(character.ClassType, character.LevelUps);

        if (!Choices.Any())
        {
            Debug.LogError("no Choices for level: " + (character.LevelUps + 1));
            return;
        }

        if (generatedClassButtons == null || generatedClassButtons.Count == 0)
        {
            generatedClassButtons = new List<Button>();

            foreach (var choice in Choices)
            {
                var clBut = Instantiate(LevelUpClassIcon, LevelUpClassIcon.transform.parent);

                //TODO: merge attribute and class images
                switch (choice.Type)
                {
                    case LevelController.ChoiceType.Attribute:
                        clBut.image.sprite = GameManager.GetAttributeImage(choice.Attribute);
                        break;
                    case LevelController.ChoiceType.Class:
                        clBut.image.sprite = GameManager.GetClassImage(choice.Class);
                        break;
                    case LevelController.ChoiceType.Skill:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                generatedClassButtons.Add(clBut);

                clBut.onClick.AddListener(() => SelectChoice(choice));

                clBut.gameObject.SetActive(true);
            }
        }
        LevelUpClassIcon.gameObject.SetActive(false);
        
        //ClassSelectText.text = "";

    }

    public void SelectChoice(LevelController.LevelUpChoice c)
    {
        SelectedChoice = c;

        switch (c.Type)
        {
            case LevelController.ChoiceType.Attribute:
                ChoiceExplanantion.text = GameManager.GetAttributeDescription(c.Attribute);
                break;
            case LevelController.ChoiceType.Class:
                ChoiceExplanantion.text = GameManager.GetClassDescription(c.Class);
                break;
            case LevelController.ChoiceType.Skill:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        ConfirmButton.interactable = true;
    }

    public void ConfirmChoice()
    {
        Debug.Log("Selection confirmed: "+SelectedChoice.Attribute);
        

        switch (SelectedChoice.Type)
        {
            case LevelController.ChoiceType.Attribute:
                LevelUp(character,character.Stats[SelectedChoice.Attribute]);
                break;
            case LevelController.ChoiceType.Class:
                SelectClass(character,SelectedChoice.Class);
                break;
            case LevelController.ChoiceType.Skill:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Close();
        
        character.LevelUps++;

        //TODO: use goblin change event
        GoblinUIList.UpdateGoblinList();

        //TODO: use a character stat change event instead.
        CharacterView.ShowCharacter(character);

        ViewHolder.SetActive(false);
    }


    private void SelectClass(Goblin c, Goblin.Class cl)
    {
        c.SelectClass(cl);
    }



    //TODO: move to character or game manager
    private void LevelUp(Goblin gob, Character.Stat stat)
    {
        stat.LevelUp();
    }
}
