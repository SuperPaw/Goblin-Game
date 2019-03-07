using System;
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

        public LocationType(Equipment.EquipLocations loc, Equipment.EquipmentType[] type)
        {
            Location = loc;
            Type = type;
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
        new AttributeDescription(Character.StatType.DAMAGE, new []{"Spiked","Bloody"},new []{"Hurt","Kill","Krank"}),
        new AttributeDescription(Character.StatType.AIM, new []{"Wooden", "Striped", "Grim"},new []{"Knark","Whack","Mok"}),
        new AttributeDescription(Character.StatType.ATTENTION, new []{"Bone","Super"},new []{"Find","Eye","Bok"}),
        new AttributeDescription(Character.StatType.COURAGE, new []{"Great","Metal","Big"},new []{"Klonk","Top"}),
        new AttributeDescription(Character.StatType.HEALTH, new []{"Strong","Brown"},new []{"Gut","Blood"}),
        new AttributeDescription(Character.StatType.SPEED, new []{"Painted", "Red"},new []{"Run","Sneak","Do"}),
        new AttributeDescription(Character.StatType.SMARTS, new []{"Smart","Black","Magic"},new []{"Brain","Head","Tink"}),
    };


    private List<AttributeDescription> NegativeAttributes = new List<AttributeDescription>()
    {
        new AttributeDescription(Character.StatType.DAMAGE, new []{"Green","Ugly"},new []{"Friend","Relax"}),
        new AttributeDescription(Character.StatType.AIM, new []{"Grey","Blue"},new []{"Beer","Drunk","Frak"}),
        new AttributeDescription(Character.StatType.ATTENTION, new []{"White", "Shiny"},new []{"Tikk","Tokk"}),
        new AttributeDescription(Character.StatType.COURAGE, new []{"Yellow","Wet"},new []{"Yield","Pak","Pok"}),
        new AttributeDescription(Character.StatType.HEALTH, new []{"Weak", "Pale"},new []{"Sokk","Sakk"}),
        new AttributeDescription(Character.StatType.SPEED, new []{ "Greasy", "Broken"},new []{"Wait","Lazy"}),
        new AttributeDescription(Character.StatType.SMARTS, new []{"Stupid","Cursed"},new []{"Dumb","Idiot"}),
    };

    private List<LocationType> LocationDescriptions = new List<LocationType>()
    {
        new LocationType(Equipment.EquipLocations.Hands,new []{Equipment.EquipmentType.Gloves}),
        new LocationType(Equipment.EquipLocations.Head,new []{Equipment.EquipmentType.Skull,Equipment.EquipmentType.Cap}),
        new LocationType(Equipment.EquipLocations.Torso,new []{Equipment.EquipmentType.Shirt,Equipment.EquipmentType.Armor,Equipment.EquipmentType.Cloth}),
        new LocationType(Equipment.EquipLocations.Weapon,new []{Equipment.EquipmentType.Stick,Equipment.EquipmentType.Sword,Equipment.EquipmentType.Blade}),
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

    public static Equipment GetRandomEquipment(Character.Race originRace = Character.Race.NoRace)
    {
        var equip = Instantiate<Equipment>(Instance.EquipmentPrefab);

        equip.EquipLocation = (Equipment.EquipLocations) Random.Range(0, (int) Equipment.EquipLocations.COUNT);

        equip.Type = Instance.LocationDescriptions.First(loc => loc.Location == equip.EquipLocation).GetClothes();


        var amountOfAttributtes = Random.Range(1, 4);
        
        Dictionary<Character.StatType,int> attributes = new Dictionary<Character.StatType, int>(amountOfAttributtes);

        for (int i = 0; i < amountOfAttributtes; i++)
        {
            Character.StatType stat;
            do
                 stat = (Character.StatType) Random.Range(0, (int) Character.StatType.COUNT);
            while 
                (attributes.ContainsKey(stat));

            if (i % 2 == 0) //positive
                attributes[stat] = Random.Range(Instance.PositiveAttributeAdjustmentMin,
                    Instance.PositiveAttributeAdjustmentMax+1);
            else
                attributes[stat] = Instance.NegativeAttributeAdjustment;
        }
        
        var shuffledList = new Dictionary<Character.StatType,int>(amountOfAttributtes);

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
            equip.name = ((originRace == Character.Race.NoRace) ? "" : originRace + " ") + equip.Type;

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

            equip.name = ((originRace == Character.Race.NoRace) ? "" : originRace + " ") + equip.Type;

            equip.name = Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, false) + "-"+ Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, false) + " " + equip.name;

            if (attributes.Count > 1)
                equip.name = Instance.GetStatDescription(attributes.ElementAt(1).Key, attributes.ElementAt(1).Value, true) + " " +
                             equip.name;

            if (attributes.Count == 3)
                equip.name += " of " + Instance.GetStatDescription(attributes.ElementAt(2).Key, attributes.ElementAt(2).Value, false) + "ing";
        }
        else
        {
            equip.name = ((originRace == Character.Race.NoRace) ? "" : originRace + " ") + equip.Type;

            equip.name = Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, true) + " " + equip.name;

            if (attributes.Count > 1)
                equip.name += " of " + Instance.GetStatDescription(attributes.ElementAt(1).Key, attributes.ElementAt(1).Value, false) + "ing";

            if (attributes.Count == 3)
                equip.name += " and " + Instance.GetStatDescription(attributes.ElementAt(2).Key, attributes.ElementAt(2).Value, false) + "ing";

        }


        foreach (var stat in attributes)  {
            //TODO: effects should not use strings
            equip.Effects.Add(new Equipment.StatEffect(stat.Key, "", new Character.Stat.StatMod(equip.name,stat.Value)));
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
