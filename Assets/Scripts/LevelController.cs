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


    [Serializable]
    public struct ClassLevelChoice
    {
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
}
