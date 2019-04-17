using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnValueHover : MonoBehaviour
{
    public Character.Stat Stat;
    public Goblin.Class Class;
    public Equipment Equipment;

    //TODO: use enum or children classes to determine type
    public bool ShowClass = false;
    public bool ShowEquipment = false;



    private string currentToolTipText = "";
    private GUIStyle guiStyleFore;
    private GUIStyle guiStyleBack;
 
    void Awake()
    {
        guiStyleFore = UIManager.Instance.HoverStyle;
        guiStyleBack = UIManager.Instance.HoverStyleBack;
        //guiStyleFore = new GUIStyle
        //{
        //    normal = {textColor = Color.black},
        //    alignment = TextAnchor.UpperCenter,
        //    wordWrap = true
        //};
        //guiStyleBack = new GUIStyle
        //{
        //    normal = {textColor = Color.white},
        //    alignment = TextAnchor.UpperCenter,
        //    wordWrap = true
        //};

    }

    public void OnMouseEnter()
    {
        //Debug.Log("Hovering: " + Stat.GetStatDescription());
        if (ShowEquipment && Equipment)
        {
            currentToolTipText = Equipment.GetEffectDescription();
        }
        else
            currentToolTipText = !ShowClass ? Stat.GetStatDescription() : GameManager.GetClassDescription(Class);
    }

    public void OnMouseExit()
    {
        currentToolTipText = "";
    }

    void OnGUI()
    {
        if (currentToolTipText != "")
        {
            var x = Event.current.mousePosition.x;
            var y = Event.current.mousePosition.y;
            GUI.Label(new Rect(x - 149, y + 40, 300, 60), currentToolTipText, guiStyleBack);
            GUI.Label(new Rect(x - 150, y + 40, 300, 60), currentToolTipText, guiStyleFore);
        }
    }
}
