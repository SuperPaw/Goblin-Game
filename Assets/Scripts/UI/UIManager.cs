using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GUIStyle HoverStyle;
    public GUIStyle HoverStyleBack;

    void Start()
    {
        if (!Instance)
            Instance = this;
    }
    
}
