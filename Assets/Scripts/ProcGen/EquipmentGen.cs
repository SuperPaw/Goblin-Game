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
        public string adjective;
        public string noun;

        public AttributeDescription(Character.StatType stat, string adjective, string noun)
        {
            Stat = stat;
            this.adjective = adjective;
            this.noun = noun;
        }
    }

    [Serializable]
    public struct LocationType
    {
        public Equipment.EquipLocations Location;
        public string Type;

        public LocationType(Equipment.EquipLocations loc, string type)
        {
            Location = loc;
            Type = type;
        }
        
    }
    //DAMAGE, AIM, ATTENTION, COURAGE, HEALTH, SPEED, SMARTS
    //TODO: maybe more fun if materials...
    public List<AttributeDescription> PositiveAttributes = new List<AttributeDescription>()
    {
        new AttributeDescription(Character.StatType.DAMAGE, "Spiked","Pain"),
        new AttributeDescription(Character.StatType.AIM, "Wood","Whack"),
        new AttributeDescription(Character.StatType.ATTENTION, "Bone","Eye"),
        new AttributeDescription(Character.StatType.COURAGE, "Heart","Guts"),
        new AttributeDescription(Character.StatType.HEALTH, "Strong","Stomach"),
        new AttributeDescription(Character.StatType.SPEED, "Painted","Toes"),
        new AttributeDescription(Character.StatType.SMARTS, "Smart","Head"),
    };


    public List<AttributeDescription> NegativeAttributes = new List<AttributeDescription>()
    {
        new AttributeDescription(Character.StatType.DAMAGE, "Dull","Friend"),
        new AttributeDescription(Character.StatType.AIM, "Grey","Beer"),
        new AttributeDescription(Character.StatType.ATTENTION, "Blind","Eye"),
        new AttributeDescription(Character.StatType.COURAGE, "Yellow","Surrender"),
        new AttributeDescription(Character.StatType.HEALTH, "Weak","Irritation"),
        new AttributeDescription(Character.StatType.SPEED, "Slow","Lazy"),
        new AttributeDescription(Character.StatType.SMARTS, "Stupid","Dumb"),
    };

    public List<LocationType> LocationDescriptions = new List<LocationType>()
    {
        new LocationType(Equipment.EquipLocations.Hands,"gloves"),
        new LocationType(Equipment.EquipLocations.Head,"hat"),
        new LocationType(Equipment.EquipLocations.Torso,"shirt"),
        new LocationType(Equipment.EquipLocations.Weapon,"club"),
    };

    //TODO: create likely stat associations for types. Like damage -> weapon. 

    private void Start()
    {
        if (!Instance)
            Instance = this;

        //TODO: check that all types are covered

        //for (int i = 0; i < 10; i++)
        //{
        //    var e = GetRandomEquipment();
        //    Debug.Log(e.name + " ; " + e.EquipLocation);
        //    foreach (var fx in e.Effects)
        //    {
        //        Debug.Log(fx.StatName + " : " + fx.Modifier.Name + " : " + fx.Modifier.Modifier);
        //    }
        //}
    }

    public static Equipment GetRandomEquipment(Character.Race originRace = Character.Race.NoRace)
    {
        var equip = Instantiate<Equipment>(Instance.EquipmentPrefab);

        equip.EquipLocation = (Equipment.EquipLocations) Random.Range(0, (int) Equipment.EquipLocations.COUNT);

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
        equip.name = ((originRace == Character.Race.NoRace) ? "" : originRace + " ") +Instance.LocationDescriptions.First(loc => loc.Location == equip.EquipLocation).Type;

        equip.name = Instance.GetStatDescription(attributes.First().Key, attributes.First().Value, true) + " "+equip.name;

        if (attributes.Count > 1)
            equip.name += " of " + Instance.GetStatDescription(attributes.ElementAt(1).Key, attributes.ElementAt(1).Value, false) ;

        if (attributes.Count == 3)
            equip.name += " and " + Instance.GetStatDescription(attributes.ElementAt(2).Key, attributes.ElementAt(2).Value, false) ;



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
                ? PositiveAttributes.First(s => s.Stat == stat).adjective
                : PositiveAttributes.First(s => s.Stat == stat).noun;
        else
        {

            return adjective
                ? NegativeAttributes.First(s => s.Stat == stat).adjective
                : NegativeAttributes.First(s => s.Stat == stat).noun;
        }
    }
}
