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
    private LevelController.LevelUpChoice[] Choices;
    private LevelController.LevelUpChoice SelectedChoice;
    [SerializeField]
    private TextMeshProUGUI ChoiceExplanantion;
    //public TextMeshProUGUI SelectText;
    private List<LevelUpChoiceEntry> generatedChoiceEntries = new List<LevelUpChoiceEntry>();
    [SerializeField]
    private LevelUpChoiceEntry levelUpChoiceEntry;
    [SerializeField]
    private LevelController.LevelUpChoice[] ClassChoices;
    [SerializeField]
    private Button ConfirmButton;
    private Vector3 normalScale = Vector3.one;
    private Vector3 HighlightScale = Vector3.one * 1.3f;


    new void Awake()
    {
        base.Awake();

        Type = WindowType.LevelUp;
    }
    public void SetupLevelScreen(Goblin character)
    {
        if(!character) return;

        Open();
        
        ConfirmButton.interactable = false;
        
        foreach (var generatedClassButton in generatedChoiceEntries)
        {
            Destroy(generatedClassButton.gameObject);
        }
        generatedChoiceEntries.Clear();

        this.character = character;
        
        ChoiceExplanantion.text = character.WaitingOnClassSelection ? "Select a Class": "Select level up";

        //TODO: use progression system instead
        Choices = character.WaitingOnClassSelection ? ClassChoices : LevelController.GetLevelUpChoices(character.ClassType, character.LevelUps);

        if (!Choices.Any())
        {
            Debug.LogError("no Choices for level: " + (character.LevelUps + 1));
            return;
        }

        if (generatedChoiceEntries == null || generatedChoiceEntries.Count == 0)
        {
            generatedChoiceEntries = new List<LevelUpChoiceEntry>();

            foreach (var choice in Choices)
            {
                var entry = Instantiate(levelUpChoiceEntry, levelUpChoiceEntry.transform.parent);

                entry.LevelUpChoice = choice;

                //TODO: merge attribute and class images
                switch (choice.Type)
                {
                    case LevelController.ChoiceType.Attribute:
                        entry.Image.sprite = GameManager.GetAttributeImage(choice.Attribute);
                        break;
                    case LevelController.ChoiceType.Class:
                        entry.Image.sprite = GameManager.GetClassImage(choice.Class);
                        break;
                    case LevelController.ChoiceType.Skill:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                generatedChoiceEntries.Add(entry);

                entry.Button.onClick.AddListener(() => SelectChoice(entry));

                entry.gameObject.SetActive(true);
            }
        }
        levelUpChoiceEntry.gameObject.SetActive(false);


        //ViewHolder.SetActive(true);


    }

    public void SelectChoice(LevelUpChoiceEntry c)
    {
        SelectedChoice = c.LevelUpChoice;

        switch (SelectedChoice.Type)
        {
            case LevelController.ChoiceType.Attribute:
                ChoiceExplanantion.text = GameManager.GetAttributeDescription(SelectedChoice.Attribute);
                break;
            case LevelController.ChoiceType.Class:
                ChoiceExplanantion.text = GameManager.GetClassDescription(SelectedChoice.Class);
                break;
            case LevelController.ChoiceType.Skill:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var g in generatedChoiceEntries.Where(gc => gc != c))
        {
            g.Image.rectTransform.localScale = normalScale;
        }

        c.Image.rectTransform.localScale = HighlightScale;
        
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


        Debug.Log("Closing level view");
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
