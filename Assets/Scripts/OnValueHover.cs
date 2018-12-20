using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class OnValueHover : MonoBehaviour
{
    public Character.Stat Stat;

    private string text = "YOUR TEXT HERE";

    private string currentToolTipText = "";
    private GUIStyle guiStyleFore;
    private GUIStyle guiStyleBack;
 
    void Start()
    {
        guiStyleFore = new GUIStyle
        {
            normal = {textColor = Color.black},
            alignment = TextAnchor.UpperCenter,
            wordWrap = true
        };
        guiStyleBack = new GUIStyle
        {
            normal = {textColor = Color.white},
            alignment = TextAnchor.UpperCenter,
            wordWrap = true
        };
        
    }

    public void OnMouseEnter()
    {
        Debug.Log("Hovering: " + Stat.GetStatDescription());

        currentToolTipText = Stat.GetStatDescription();
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
