using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootable : MonoBehaviour
{
    public bool ContainsLoot = true;
    public string Loot;
    public bool Searched;

    public void Start()
    {
        //TODO: could be handled when found, so it 
        Loot = NameGenerator.GetTreasureName();
    }
}
