using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Serializable]
    public struct EquipEntry
    {
        //public Equipment.EquipLocations Location;
        public Equipment.EquipmentType Type;
        public Item[] Items;
    }

    public EquipEntry[] Equipments;

    public void Show(Equipment.EquipmentType type)
    {
        foreach (var eq in Equipments.Where(e => e.Type == type))
        {
            foreach (var eqItem in eq.Items)
            {
                eqItem.gameObject.SetActive(true);
            }
        }
    }

    public void Hide(Equipment.EquipmentType type)
    {

        foreach (var eq in Equipments.Where(e => e.Type == type))
        {
            foreach (var eqItem in eq.Items)
            {
                eqItem.gameObject.SetActive(false);
            }
        }
    }
}
