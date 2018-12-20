using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Environments;
using UnityEngine;
using UnityEngine.UI;

public class PlayerChoice : MonoBehaviour
{
    public struct ChoiceOption
    {
        public string Description;
        public Action Action;
    }

    public static PlayerChoice Instance;
    private ChoiceOption defaultOption;
    private bool ActionSelected;
    private ChoiceOption[] options;
    public GameObject ViewHolder;
    //TODO: could have a related character view 
    public Button OptionButtonEntry;
    private List<Button> generatedObjects = new List<Button>();
    public Text DescriptionText;

    void Start()
    {
        if (!Instance)
            Instance = this;
    }

    //a list of options 
    //first option is the default
    public static void SetupPlayerChoice(ChoiceOption[] choices, string optionText)
    {
        if (choices.Length < 1) return;
        
        Instance.options = choices;
        Instance.DescriptionText.text = optionText;

        Instance.OpenWindow();
    }

    private void OpenWindow()
    {
        ViewHolder.SetActive(true);

        ActionSelected = false;

        OptionButtonEntry.gameObject.SetActive(false);
        
        foreach (var choice in options)
        {
            if (choice.Action == null)
            {
                Debug.Log("No action set for choice: "+ choice.Description);
            }

            var entry = Instantiate(OptionButtonEntry, OptionButtonEntry.transform.parent);

            entry.gameObject.SetActive(true);

            entry.GetComponentInChildren<Text>().text = choice.Description;

            var o = choice;

            entry.onClick.AddListener(() =>SelectOption(o));

            generatedObjects.Add(entry);
        }
    }

    private void SelectOption(ChoiceOption o)
    {
        if (!ActionSelected)
        {
            Debug.Log("Selected optino: " + o.Description);

            o.Action.Invoke();

            ActionSelected = true;
            Close();
        }

    }

    public void Close()
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
        ViewHolder.SetActive(false);
    }
}
