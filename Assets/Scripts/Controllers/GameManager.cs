using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool DebugText;
    public bool GameStarted;
    public bool GamePaused;
    public bool InvincibleMode;
    public bool DisableUnseenCharacters;
    public GameObject[] TurnOnOnStart;
    public AnimationCurve HitChance;

    public enum Icon
    {
        LevelUp, Chief, Healing, Idling, Fleeing, Travelling, Fighting, Hiding, Dead, Watching, Searching, Provoking, Resting,
        Unknown
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
    [Serializable]
    public struct AttributeImg
    {
        public Character.StatType Stat;
        public Sprite Image;
        //TODO: use this instead of on the stat
        public string Description;
    }
    [Serializable]
    public struct TargetImaget
    {
        public PointOfInterest.OptionType Type;
        public Sprite Image;
        public string DefaultQuestionText;
    }

    [Serializable]
    public struct EquipmentImage
    {
        public Equipment.EquipmentType Type;
        public Sprite Sprite;
    }

    [Serializable]
    public struct HappinessImage
    {
        public Goblin.Happiness Type;
        public Sprite Sprite;
        public Color Color;
    }

    [Serializable]
    public struct EnemyImage
    {
        public Character.Race Type;
        public Sprite Sprite;
    }


    [Header("References")]
    public EnemyImage[] EnemyImages;
    public IconImg[] IconImages;
    public ClassImg[] ClassImgs;
    public EquipmentImage[] EquipmentImages;
    public AttributeImg[] AttributeImages;
    public HappinessImage[] HappinessImages;
    public List<TargetImaget> OptionTargetImages;
    public Image BlackscreenImage;
    public GameObject HighScoreScreen;

    [Header("Game Rules")]
    public int XpOnKill = 10;
    public int XpOnTeamKill = 2;
    //TODO: implement
    public int XpOnTreasureFind = 1;
    public int XpOnAreaMove = 1;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }

    public static Sprite GetIconImage(Icon type)
    {
        if (Instance.IconImages.Any(im => im.IconType == type))
        {
            return Instance.IconImages.First(im => im.IconType == type).Image;
        }

        return null;
    }

    public static Sprite GetClassImage(Goblin.Class type)
    {
        if (Instance.ClassImgs.Any(im => im.ClassType == type))
        {
            return Instance.ClassImgs.First(im => im.ClassType == type).Image;
        }

        return Instance.ClassImgs.First().Image;
    }

    public static Sprite GetClassImage(Goblin goblin)
    {
        var type = goblin.ClassType;

        if(!goblin.Alive())
            return GameManager.GetIconImage(GameManager.Icon.Dead);

        if (type == Goblin.Class.Goblin)
        {
            return goblin.GoblinImage;
        }

        if (Instance.ClassImgs.Any(im => im.ClassType == type))
        {
            return Instance.ClassImgs.First(im => im.ClassType == type).Image;
        }

        return Instance.ClassImgs.First().Image;
    }
    public static Sprite GetAttributeImage(Character.StatType type)
    {
        if (Instance.AttributeImages.Any(im => im.Stat == type))
        {
            return Instance.AttributeImages.First(im => im.Stat == type).Image;
        }

        return Instance.AttributeImages.First().Image;
    }

    public static HappinessImage GetSmiley(Goblin.Happiness type)
    {
        if (Instance.HappinessImages.Any(im => im.Type == type))
        {
            return Instance.HappinessImages.First(im => im.Type == type);
        }

        return Instance.HappinessImages.First();
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static string GetClassDescription(Goblin.Class type)
    {
        if (Instance.ClassImgs.Any(im => im.ClassType == type))
        {
            return Instance.ClassImgs.First(im => im.ClassType == type).Description;
        }

        return "Goblins are cool!!";
    }

    public static string GetAttributeDescription(Character.StatType type)
    {
        if (Instance.AttributeImages.Any(im => im.Stat == type))
        {
            return Instance.AttributeImages.First(im => im.Stat == type).Description;
        }

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

    internal static Sprite GetIconImage(Equipment.EquipmentType type)
    {

        if (Instance.EquipmentImages.Any(im => im.Type == type))
        {
            return Instance.EquipmentImages.First(im => im.Type == type).Sprite;
        }

        return Instance.EquipmentImages.First().Sprite;
    }

    public static int XpKill => Instance.XpOnKill;
    public static int XpTeamKill => Instance.XpOnTeamKill;

    internal static void GameOver(bool gameWon = false)
    {
        if (gameWon)
        {
            SoundController.PlayStinger(SoundBank.Stinger.GameWin);
        }
        else
        {
            SoundController.PlayGameLoss();
        }

        //tODO: add points to score if won

        Instance.StartCoroutine(Instance.FadeToHighScoreRoutine());
    }

    internal static void StartGame()
    {
        foreach (var go in Instance.TurnOnOnStart)
        {
            go.SetActive(true);
        }

        FindObjectOfType<PlayerController>().Initialize();

        //SoundController.PlayGameStart();

        SoundController.ChangeMusic(SoundBank.Music.Explore);

        Instance.GameStarted = true;

    }

    public static float GetHitChance(int i)
    {
        return Instance.HitChance.Evaluate(i);
    }

    public void PauseButton()
    {
        if (GamePaused)
        {
            UnPause();
        }
        else
        {
            Pause();
        }
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

    public IEnumerator FadeToHighScoreRoutine()
    {
        var start = Time.time;
        var duration = 6f;
        var end = start + duration;

        var endColor = BlackscreenImage.color;
        var startColor = new Color(0, 0, 0, 0);

        BlackscreenImage.gameObject.SetActive(true);

        while (Time.time < end)
        {
            BlackscreenImage.color = Color.Lerp(startColor, endColor, (Time.time - start) / duration);
            yield return new WaitForFixedUpdate();
        }

        GreatestGoblins.ShowHighscores();
        Instance.GameStarted = false;
        //TODO: move to above method
        HighScoreScreen.gameObject.SetActive(true);
    }
}
