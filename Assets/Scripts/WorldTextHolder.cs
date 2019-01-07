using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldTextHolder : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI MapTextEntry;
    [SerializeField]
    private Text FloatTextEntry;

    private static WorldTextHolder Instance;

    [SerializeField]
    private Text ValueChangeText;
    [SerializeField]
    private Color PositiveChangeColor = Color.green;
    [SerializeField]
    private Color NegativeChangeColor = Color.red;

    private void Start()
    {
        if (!Instance)
            Instance = this;
    }


    public static TextMeshProUGUI CreateNewMapEntry()
    {
        return Instance.createNewMapEntry();
    }

    private TextMeshProUGUI createNewMapEntry()
    {
        return GameObject.Instantiate(MapTextEntry,this.transform);
    }
    
    public Text CreateNewFloatEntry()
    {
        return GameObject.Instantiate(FloatTextEntry, this.transform);
    }

    public Text CreateNewValueChangeEntry(bool positive)
    {
        var entry = GameObject.Instantiate(ValueChangeText, this.transform);

        entry.color = positive ? PositiveChangeColor : NegativeChangeColor;

        return entry;
    }
}
