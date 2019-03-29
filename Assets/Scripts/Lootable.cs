using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootable : MonoBehaviour
{
    public bool ContainsLoot = true;
    public string Loot;
    public bool Searched;
    public bool ContainsFood;
    public string Food;
    public List<Equipment> EquipmentLoot = new List<Equipment>();

    public Area InArea;

    public void Start()
    {
        //TODO: could be handled when found, so it 
        Loot = NameGenerator.GetTreasureName();
    }
}
