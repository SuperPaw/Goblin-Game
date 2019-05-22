using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

public class LegacySystem : MonoBehaviour
{
    private static LegacySystem Instance;

    public class AchievementUnlockedEvent : UnityEvent<Achievement> { }

    public AchievementUnlockedEvent OnUnlock = new AchievementUnlockedEvent();

    [Serializable]
    public class Achievement
    {
        public string Name;
        public UnlockCondition Condition;
        public int AmountToUnlock;
        public int X;
        public bool Unlocked;
        public bool VisibleUnlockCondition;
        public Sprite Image;
        //using string due to goblin.Class being serialized wrongly
        public Goblin.Class UnlocksClass;
        public MapGenerator.WorldSize UnlocksMapSize;
        public Blessing UnlocksBlessing;
    }

    [SerializeField] private List<Achievement> Achievements;
    
    public enum UnlockCondition { Kills, Equipment,GoblinSacrifice,Treasure}
    public enum Blessing { NoBlessing, Xp,Health,ExtraGoblin,Smarts}
        

    void Awake()
    {
        if (!Instance) Instance = this;

        OnUnlock.AddListener(a => SoundController.PlayStinger(SoundBank.Stinger.AchievementUnlocked));
        //OnUnlock.AddListener(PopupAchievement.ShowAchievement);
    }

    public static void SetAchievements(List<Achievement> ass)
    {
        Instance.Achievements = ass;
    }
    public static List<Achievement> GetAchievements()
    {
        return Instance.Achievements;
    }

    //TODO: will not work due to struct
    private void CountUp(Achievement a)
    {
        a.X++;
        if (a.X >= a.AmountToUnlock)
        {
            a.Unlocked = true;
            OnUnlock.Invoke(a);
        }
    }

}
