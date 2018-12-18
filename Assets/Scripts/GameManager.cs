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
    [Header("Images")]
    public IconImg[] IconImages;
    public ClassImg[] ClassImgs;

    [Header("Game Rules")]
    public int XpOnKill = 10;
    public int XpOnTeamKill = 2;


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

    public static Sprite GetIconImage(Character.CharacterState state)
    {
        switch (state)
        {
            case Character.CharacterState.Idling:
                return GetIconImage(Icon.Idling);
            case Character.CharacterState.Attacking:
                return GetIconImage(Icon.Fighting);
            case Character.CharacterState.Travelling:
                return GetIconImage(Icon.Travelling);
            case Character.CharacterState.Fleeing:
                return GetIconImage(Icon.Fleeing);
            case Character.CharacterState.Hiding:
                return GetIconImage(Icon.Hiding);
            case Character.CharacterState.Dead:
                return GetIconImage(Icon.Dead);
            default:
                throw new ArgumentOutOfRangeException("state", state, null);
        }
        
    }

    public static int XpKill()
    {
        return Instance.XpOnKill;
    }
    public static int XpTeamKill()
    {
        return Instance.XpOnKill;
    }
}
