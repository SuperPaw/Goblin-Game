
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum Icon { LevelUp, Chief, Healing, Idling,Fleeing,Travelling,Fighting,Hiding, Dead}
    [Serializable]
    public struct IconImg
    {
        public Icon IconType;
        public Sprite Image;
    }
    
    public IconImg[] IconImages;

    void Start()
    {
        if (!Instance) Instance = this;
    }

    public static Sprite GetIconImage(Icon type)
    {
        if(Instance.IconImages.Any(im => im.IconType == type))
            return Instance.IconImages.First(im => im.IconType == type).Image;

        return null;
    }
}
