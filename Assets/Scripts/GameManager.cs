using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public bool GameStarted;
    public bool GamePaused;
    public bool InvincibleMode;
    
    public enum Icon
    {
        LevelUp, Chief, Healing, Idling, Fleeing, Travelling, Fighting, Hiding, Dead,Watching,Searching, Provoking, Resting
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
        public string Description;
    }
    [Header("References")]
    public IconImg[] IconImages;
    public ClassImg[] ClassImgs;

    [Header("Game Rules")]
    public int XpOnKill = 10;
    public int XpOnTeamKill = 2;
    //TODO: implement
    public int XpOnTreasureFind = 1;
    public int XpOnAreaMove = 1;
    
    void Awake()
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

    public static string GetClassDescription(Goblin.Class type)
    {
        if (Instance.ClassImgs.Any(im => im.ClassType == type))
            return Instance.ClassImgs.First(im => im.ClassType == type).Description;
        return "Goblins are cool!!";
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
            case Character.CharacterState.Watching:
                return GetIconImage(Icon.Watching);
            case Character.CharacterState.Searching:
                return GetIconImage(Icon.Searching);
            case Character.CharacterState.Provoking:
                return GetIconImage(Icon.Provoking);
            case Character.CharacterState.Surprised:
                return GetIconImage(Icon.Fleeing);
            case Character.CharacterState.Resting:
                return GetIconImage(Icon.Resting);
            default:
                return GetIconImage(Icon.Idling);
        }
        
    }

    public static int XpKill()
    {
        return Instance.XpOnKill;
    }
    public static int XpTeamKill()
    {
        return Instance.XpOnTeamKill;
    }

    internal static void GameOver()
    {
        Instance.GameStarted = false;

        SoundController.PlayGameLoss();
    }

    public void PauseButton()
    {
        if(GamePaused)
            UnPause();
        else
            Pause();
    }

    public static void Pause()
    {
        //TODO: add grayscale layer
        Time.timeScale = 0f;
        Instance.GamePaused = true;
    }

    public static void UnPause()
    {
        Instance.GamePaused = false;
        Time.timeScale = 1f;
    }
}
