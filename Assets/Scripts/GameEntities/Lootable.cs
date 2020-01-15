using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootable : MonoBehaviour
{
    public bool ContainsLoot = true;
    public Treasure Loot;
    public bool Searched;
    public bool ContainsFood;
    public Treasure Food;
    public List<Equipment> EquipmentLoot = new List<Equipment>();

    [Serializable]
    public struct Treasure
    {
        public string Name;
        public Sprite LootImage;
        
    }

    public Area InArea;

    public void Start()
    {
        Loot = NameGenerator.GetTreasureName();
    }
}
