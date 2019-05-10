using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    //Goblin | Slave | Swarmer |Shooter |Ambusher | Scout
    public ClassLevelChoice[] SlaveProgression,
        SwarmerProgression,
        ShooterProgression,
        AmbusherProgression,
        ScoutProgression;

    public static LevelController Instance;

    void Awake()
    {
        if (!Instance) Instance = this;
    }

    [Serializable]
    public struct ClassLevelChoice
    {
        public ChoiceType Type;
        public LevelUpChoice[] Choices;
    }

    public enum ChoiceType { Attribute, Class, Skill }

    //TODO: maybe move?
    [Serializable]
    public struct LevelUpChoice
    {
        public LevelController.ChoiceType Type;
        public Character.StatType Attribute;
        public Goblin.Class Class;
    }

    public static LevelUpChoice[] GetLevelUpChoices(Goblin.Class classType, int level)
    {
        return Instance.SlaveProgression[level].Choices;
    }
}
