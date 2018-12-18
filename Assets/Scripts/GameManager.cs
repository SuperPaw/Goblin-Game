using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum Icon
    {
        LevelUp, Chief, Healing, Idling, Fleeing, Travelling, Fighting, Hiding, Dead,
    }
    [Serializable]
    public struct IconImg
    {
        public Icon IconType;
        public Sprite Image;
    }

    [Serializable]
    public struct ClassImg
    {
        public Goblin.Class ClassType;
        public Sprite Image;
    }
    public IconImg[] IconImages;
    public ClassImg[] ClassImgs;

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
    public static Sprite GetClassImage(Goblin.Class type)
    {
        if (Instance.ClassImgs.Any(im => im.ClassType == type))
            return Instance.ClassImgs.First(im => im.ClassType == type).Image;

        return Instance.ClassImgs.First().Image;
    }
}
