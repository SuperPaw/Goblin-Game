﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

public class EquipmentGen : MonoBehaviour
{
    public Equipment EquipmentPrefab;
    public static EquipmentGen Instance;

    public int PositiveAttributeAdjustmentMin = 2;
    public int PositiveAttributeAdjustmentMax = 3;
    public int NegativeAttributeAdjustment = -1;

    [Serializable]
    public struct AttributeDescription
    {
        public Character.StatType Stat;
        public string[] adjective;
        public string[] noun;

        public AttributeDescription(Character.StatType stat, string[] adjective, string[] noun)
        {
            Stat = stat;
            this.adjective = adjective;
            this.noun = noun;
        }

        public string GetNoun()
        {
            return noun[Random.Range(0, noun.Length)];
        }
        public string GeAdjective()
        {
            return adjective[Random.Range(0, adjective.Length)];
        }
    }

    [Serializable]
    public struct LocationType
    {
        public Equipment.EquipLocations Location;
        public Equipment.EquipmentType[] Type;
        public Goblin.Class UsableBy;

        public LocationType(Equipment.EquipLocations loc, Equipment.EquipmentType[] type, Goblin.Class usableBy = Goblin.Class.ALL)
        {
            Location = loc;
            Type = type;
            this.UsableBy = usableBy;
        }


        public Equipment.EquipmentType GetClothes()
        {
            return Type[Random.Range(0, Type.Length)];
        }

    }
    //DAMAGE, AIM, ATTENTION, COURAGE, HEALTH, SPEED, SMARTS
    //TODO: maybe more fun if materials...
    private List<AttributeDescription> PositiveAttributes = new List<AttributeDescription>()
    {
        new AttributeDescription(Character.StatType.DAMAGE, new []{ "Red","Painted", "Spiked", "Bloody"},new []{"Hurt","Kill","Krank"}),
        new AttributeDescription(Character.StatType.AIM, new []{ "Super","Wooden", "Striped"},new []{"Knark","Whack","Mok","Do"}),
        new AttributeDescription(Character.StatType.COURAGE, new []{"Great","Metal","Big"},new []{"Klonk","Top"}),
        new AttributeDescription(Character.StatType.HEALTH, new []{ "Bone","Strong","Brown"},new []{"Gut","Blood","Bok"}),
        new AttributeDescription(Character.StatType.SMARTS, new []{"Smart","Black","Magic"},new []{ "Find", "Eye","Brain","Head","Tink"}),
    };


    private List<AttributeDescription> NegativeAttributes = new List<AttributeDescription>()
    {
        new AttributeDescription(Character.StatType.DAMAGE, new []{"Green","Ugly"},new []{"Friend","Relax","Tokk"}),
        new AttributeDescription(Character.StatType.AIM, new []{"Broken" , "Greasy", "Grey", "Blue"},new []{"Beer","Drunk","Frak"}),
        new AttributeDescription(Character.StatType.COURAGE, new []{"Yellow","Wet"},new []{ "Wait", "Yield","Pak","Pok"}),
        new AttributeDescription(Character.StatType.HEALTH, new []{"Weak", "Pale"},new []{"Sokk","Sakk"}),
        new AttributeDescription(Character.StatType.SMARTS, new []{ "White", "Stupid","Cursed","Shiny"},new []{ "Tikk", "Dumb", "Idiot" }),
    };

    private List<LocationType> LocationDescriptions = new List<LocationType>()
    {
        new LocationType(Equipment.EquipLocations.Hands,new []{Equipment.EquipmentType.Gloves}),
        new LocationType(Equipment.EquipLocations.Head,new []{Equipment.EquipmentType.Skull,Equipment.EquipmentType.Cap}),
        new LocationType(Equipment.EquipLocations.Torso,new []{Equipment.EquipmentType.Shirt,Equipment.EquipmentType.Armor,Equipment.EquipmentType.Cloth}),
        new LocationType(Equipment.EquipLocations.Weapon,new []{Equipment.EquipmentType.Stick,Equipment.EquipmentType.Sword,Equipment.EquipmentType.Blade}, Goblin.Class.ALL & (~Goblin.Class.Shooter) ),
        new LocationType(Equipment.EquipLocations.Bow,new []{Equipment.EquipmentType.Bow},Goblin.Class.Shooter),
        new LocationType(Equipment.EquipLocations.Feet,new []{Equipment.EquipmentType.Boots,Equipment.EquipmentType.Shoes}),
    };

    //TODO: create likely stat associations for types. Like damage -> weapon. 

    private void Awake()
    {
        if (!Instance)
            Instance = this;

        //TODO: check that all types are covered

        //for (int i = 0; i < 20; i++)
        //{
        //    var e = GetRandomEquipment();
        //    Debug.Log(e.name + " ; " + e.EquipLocation);
        //    //foreach (var fx in e.Effects)
        //    //{
        //    //    Debug.Log(fx.Stat + " : " + fx.Modifier.Name + " : " + fx.Modifier.Modifier);
        //    //}
        //}
    }

    public static Equipment GetRandomEquipment()
    {
        var e = (Equipment.EquipLocations) Random.Range(0, (int) Equipment.EquipLocations.COUNT);

        return GetEquipment(Instance.LocationDescriptions.First(loc => loc.Location == e).GetClothes());
        
    }

    public static Equipment GetEquipment(Equipment.EquipmentType type)
    {
        var equip = Instantiate(Instance.EquipmentPrefab);

        equip.Type = type;

        List<Character.StatType> affectableStats = new List<Character.StatType>()
        {
            Character.StatType.COURAGE,Character.StatType.DAMAGE,Character.StatType.AIM,Character.StatType.SMARTS,Character.StatType.HEALTH
        };

        equip.EquipLocation = Instance.LocationDescriptions.First(loc => loc.Type.Contains(type)).Location;


        equip.UsableBy = Instance.LocationDescriptions.First(loc => loc.Location == equip.EquipLocation).UsableBy;

        var amountOfAttributtes = Random.Range(1, 4);

        Dictionary<Character.StatType, int> attributes = new Dictionary<Character.StatType, int>(amountOfAttributtes);

        for (int i = 0; i < amountOfAttributtes; i++)
        {
            Character.StatType stat;
            do
                stat = affectableStats[Random.Range(0, affectableStats.Count)];
            while
                (attributes.ContainsKey(stat));

            if (i % 2 == 0) //positive
                attributes[stat] = Random.Range(Instance.PositiveAttributeAdjustmentMin,
                    Instance.PositiveAttributeAdjustmentMax + 1);
            else
                attributes[stat] = Instance.NegativeAttributeAdjustment;
        }

        var shuffledList = new Dictionary<Character.StatType, int>(amountOfAttributtes);

        while (attributes.Any())
        {
            var next = attributes.ElementAt(Random.Range(0, attributes.Count));

            shuffledList[next.Key] = next.Value;

            attributes.Remove(next.Key);
        }

        attributes = shuffledList;

        //NAME GENERATION
        var x = Random.value;
        if (x < 0.33f)
        {
            equip.name = equip.Type.ToString();

            equip.name = Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, false) + " " +
                         equip.name;

            if (attributes.Count > 1)
                equip.name = Instance.GetStatDescription(attributes.ElementAt(1).Key, attributes.ElementAt(1).Value, false) + "-" +
                             equip.name;

            if (attributes.Count == 3)
                equip.name = Instance.GetStatDescription(attributes.ElementAt(2).Key, attributes.ElementAt(2).Value, true) + " " +
                             equip.name;
        }
        else if (x < 0.67)
        {

            equip.name = "" + equip.Type;

            equip.name = Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, false) + "-" + Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, false) + " " + equip.name;

            if (attributes.Count > 1)
                equip.name = Instance.GetStatDescription(attributes.ElementAt(1).Key, attributes.ElementAt(1).Value, true) + " " +
                             equip.name;

            if (attributes.Count == 3)
                equip.name += " of " + Instance.GetStatDescription(attributes.ElementAt(2).Key, attributes.ElementAt(2).Value, false) + "ing";
        }
        else
        {
            equip.name = "" + equip.Type;

            equip.name = Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, true) + " " + equip.name;

            if (attributes.Count > 1)
                equip.name += " of " + Instance.GetStatDescription(attributes.ElementAt(1).Key, attributes.ElementAt(1).Value, false) + "ing";

            if (attributes.Count == 3)
                equip.name += " and " + Instance.GetStatDescription(attributes.ElementAt(2).Key, attributes.ElementAt(2).Value, false) + "ing";

        }


        foreach (var stat in attributes)
        {
            //TODO: effects should not use strings
            equip.Effects.Add(new Equipment.StatEffect(stat.Key, "", new Character.Stat.StatMod(equip.name, stat.Value)));
        }

        return equip;
    }

    private string GetStatDescription(Character.StatType stat, int modifier, bool adjective)
    {
        if (modifier > 0)
            return adjective
                ? PositiveAttributes.First(s => s.Stat == stat).GeAdjective()
                : PositiveAttributes.First(s => s.Stat == stat).GetNoun();
        else
        {

            return adjective
                ? NegativeAttributes.First(s => s.Stat == stat).GeAdjective()
                : NegativeAttributes.First(s => s.Stat == stat).GetNoun();
        }
    }
}
