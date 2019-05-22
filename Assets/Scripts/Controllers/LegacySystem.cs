using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LegacySystem : MonoBehaviour
{
    private static LegacySystem Instance;

    [Serializable]
    public struct Achievement
    {
        public string Name;
        public UnlockCondition Condition;
        public int AmountToUnlock;
        public bool Unlocked;
        public bool VisibleUnlockCondition;
        public Sprite Image;
        public Goblin.Class UnlocksClass;
        public MapGenerator.WorldSize UnlocksMapSize;
        public Blessing UnlocksBlessing;
    }

    [SerializeField] private List<Achievement> Achievements = null;
    
    public enum UnlockCondition { Kills, Equipment,GoblinSacrifice}
    public enum Blessing { NoBlessing, Xp,Health,ExtraGoblin,Smarts}
        

    void Start()
    {
        if (!Instance) Instance = this;
    }

    public static void SetAchievements(List<Achievement> ass)
    {
        Instance.Achievements = ass;
    }
    public static List<Achievement> GetAchievements()
    {
        return Instance.Achievements;
    }

}
