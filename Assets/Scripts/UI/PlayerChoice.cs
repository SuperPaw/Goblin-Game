using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChoice : MenuWindow
{
    public struct ChoiceOption
    {
        public string Description;
        public Action Action;

        public ChoiceOption(string desc, Action ac)
        {
            Description = desc;
            Action = ac;
        }
    }

    public static PlayerChoice Instance;
    private ChoiceOption defaultOption;
    private bool ActionSelected;
    private ChoiceOption[] options;
    //TODO: could have a related character view 
    public Button OptionButtonEntry;
    private List<Button> generatedObjects = new List<Button>();
    public TextMeshProUGUI DescriptionText;
    public ChoiceOption NoOption = new ChoiceOption("No", null);

    new void Awake()
    {
        base.Awake();

        Type = WindowType.PlayerChoice;

        if (!Instance)
            Instance = this;
    }

    //a list of options 
    //first option is the default
    public static void SetupPlayerChoice(ChoiceOption[] choices, string optionText)
    {
        if (choices.Length < 1) return;
        
        
        Instance.StartCoroutine( Instance.OpenWhenReady(choices,optionText));
    }

    public static void CreateDoChoice(Action doAction, string question)
    {
        var choices = new ChoiceOption[] {new ChoiceOption("Gob gob",doAction),Instance.NoOption };


        Instance.StartCoroutine(Instance.OpenWhenReady(choices, question));
    }

    //private void OpenWindow()
    //{
    //    StartCoroutine(OpenWhenReady());
    //}

    private IEnumerator OpenWhenReady(ChoiceOption[] choices, string optionText)
    {
        yield return new WaitUntil(() => !OpenWindows[Type]);

        options = choices;
        DescriptionText.text = optionText;

        SoundController.PlayMenuPopup();

        Open();

        defaultOption = options.First();

        ActionSelected = false;

        OptionButtonEntry.gameObject.SetActive(false);

        foreach (var choice in options)
        {
            if (choice.Action == null)
            {
                //Debug.Log("No action set for choice: "+ choice.Description);
            }

            var entry = Instantiate(OptionButtonEntry, OptionButtonEntry.transform.parent);

            entry.gameObject.SetActive(true);

            entry.GetComponentInChildren<Text>().text = choice.Description;

            var o = choice;

            entry.onClick.AddListener(() => SelectOption(o));

            generatedObjects.Add(entry);
        }
    }

    private void SelectOption(ChoiceOption o)
    {
        if (!ActionSelected)
        {
            //Debug.Log("Selected optino: " + o.Description);

            if(o.Action != null)
                o.Action.Invoke();

            ActionSelected = true;
            CloseWindow();
        }

    }

    public void CloseWindow()
    {
        if (!ActionSelected)
        {
            defaultOption.Action.Invoke();

            ActionSelected = true;
        }

        //Clean up
        generatedObjects.ForEach(b=> Destroy(b.gameObject));
        generatedObjects.Clear();

        //close
        Close();
    }
}
