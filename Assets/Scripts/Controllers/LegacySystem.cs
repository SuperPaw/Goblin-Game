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

    public class UnlockConditionEvent : UnityEvent<UnlockCondition> { }
    public static UnlockConditionEvent OnConditionEvent = new UnlockConditionEvent();

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
        public bool Unlockable = true;
        public bool VisibleUnlockCondition;
        public Sprite Image;
        //using string due to goblin.Class being serialized wrongly
        public string UnlocksClass;
        public MapGenerator.WorldSize UnlocksMapSize;
        public Blessing UnlocksBlessing;
    }

    [SerializeField] private List<Achievement> Achievements;
    
    public enum UnlockCondition { KillGiant,
        KillSpider,
        KillZombie,
        KillHuman,
        KillWolf,
        EquipmentFound,
        GoblinSacrifice,
        GoblinDeath,
        Treasure,
        DestroyFarm,
        FindANewHome
    }
    public enum Blessing { NoBlessing, Xp,Food,Treasure,Health,ExtraGoblin,ExtraSlaves,Smarts,Damage,Speed,Aim,SoloGoblin}
        

    void Awake()
    {
        if (!Instance) Instance = this;

        OnUnlock.AddListener(AchievementPopup.ShowAchievement);

        OnConditionEvent.AddListener(HandleConditionIncrement);

        Character.OnAnyCharacterDeath.AddListener(RaceDeath);
    }

    public static void SetAchievements(List<Achievement> ass)
    {
        foreach (var a in ass)
        {
            Instance.UpdateAchievement(a);
        }
    }

    private void UpdateAchievement(Achievement a)
    {
        var old = Achievements.First(o => o.Name == a.Name);

        old.Unlocked = a.Unlocked;
        old.X = a.X;
    }

    public static List<Achievement> GetAchievements()
    {
        return Instance.Achievements;
    }

    private void RaceDeath(Character.Race r)
    {
        switch (r)
        {
            case Character.Race.Goblin:
                OnConditionEvent.Invoke(UnlockCondition.GoblinDeath);
                break;
            case Character.Race.Human:
                OnConditionEvent.Invoke(UnlockCondition.KillHuman);
                break;
            case Character.Race.Spider:
                OnConditionEvent.Invoke(UnlockCondition.KillSpider);
                break;
            case Character.Race.Zombie:
                OnConditionEvent.Invoke(UnlockCondition.KillZombie);
                break;
            case Character.Race.Ogre:
                OnConditionEvent.Invoke(UnlockCondition.KillGiant);
                break;
            case Character.Race.Wolf:
                OnConditionEvent.Invoke(UnlockCondition.KillWolf);
                break;
            case Character.Race.NoRace:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(r), r, null);
        }
    }

    //TODO: will not work due to struct
    private void CountUp(Achievement a)
    {
        a.X++;
        if (!a.Unlocked && a.X >= a.AmountToUnlock && a.Unlockable)
        {
            a.Unlocked = true;
            OnUnlock.Invoke(a);
        }
    }

    private void HandleConditionIncrement(UnlockCondition c)
    {
        foreach (var achievement in Achievements.Where(a=> a.Condition == c))
        {
            CountUp(achievement);
        }
    }
}
