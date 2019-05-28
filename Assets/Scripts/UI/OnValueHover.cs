using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnValueHover : Button
{
    public Character.Stat Stat;
    public Goblin.Class Class;
    public Equipment Equipment;

    //TODO: use enum or children classes to determine type
    public bool ShowClass = false;
    public bool ShowEquipment = false;

    private new void Start()
    {
        onClick.AddListener(ShowInfo);
    }
    
    void ShowInfo()    {
        Debug.Log("Opening: " + gameObject);
        if (ShowEquipment && Equipment)
        {
            InfoClick.ShowInfo(gameObject.GetComponent<RectTransform>(), Equipment.name,
                Equipment.GetEffectDescription());
        }
        else if (ShowClass)
        {
            InfoClick.ShowInfo(gameObject.GetComponent<RectTransform>(), Class.ToString(),
                GameManager.GetClassDescription(Class), GameManager.GetClassImage(Class));
        }
        else if(Stat!=null)
        {
            InfoClick.ShowInfo(gameObject.GetComponent<RectTransform>(), Stat.Type.ToString(),
                Stat.GetStatDescription(), GameManager.GetAttributeImage(Stat.Type));
        }

        Debug.LogError("No description to show: "+ gameObject);
    }
    
}
