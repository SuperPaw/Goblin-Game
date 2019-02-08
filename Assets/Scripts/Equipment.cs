using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Equipment : MonoBehaviour
{
    public enum EquipLocations
    {
        Head, Torso, Weapon, Hands,
        COUNT
    }
    //public abstract class Effect{}

    [Serializable]
    public struct StatEffect
    {
        public Character.Stat.StatMod Modifier;
        //eg. Damage
        public Character.StatType Stat;
        //eg. DMG
        //public string StatNameShort;
        //public Character.Race WhenAttacking;

        public StatEffect(Character.StatType statName, string statNameShort, Character.Stat.StatMod modifier)
        {
            Modifier = modifier;
            Stat = statName;
        }
    }
    
    public class EquipEvent : UnityEvent<Character> { }
    public EquipEvent OnEquip = new EquipEvent();
    public EquipEvent OnDeequip = new EquipEvent();

    public struct TeamEffect{}

    public Sprite Icon;
    //Maybe 3d Model??
    public Sprite Model;

    public EquipLocations EquipLocation;

    public List<StatEffect> Effects = new List<StatEffect>();

    internal string GetEffectDescription()
    {
        return Effects.Aggregate("", (current, fx) => current + (fx.Modifier.Modifier + " " + fx.Stat + ". "));
    }

    //TODO: use this for instance for bows
    public Goblin.Class UsableBy;
    
    void Start()
    {
        OnEquip.AddListener(Equip);
        OnDeequip.AddListener(DeEquip);
    }
    

    void Equip(Character c)
    {
        foreach (var fx in Effects)
        {
            Character.Stat st;

            if (fx.Stat == Character.StatType.HEALTH)
                st = c.HEA;
            else
            {
                if (!c.Stats.ContainsKey(fx.Stat))
                {
                    Debug.LogWarning(fx.Stat + ": is not a real stat!");
                    continue;
                }

                st = c.Stats[fx.Stat];
            }
            //Debug.Log("Adding equipment modifier : "+ name +", "+ st.Name + "; " + fx.Modifier.Modifier);
            st.Modifiers.Add(fx.Modifier);
        }

        transform.parent = c.transform;
    }

    void DeEquip(Character c)
    {
        foreach (var fx in Effects)
        {
            var st = fx.Stat == Character.StatType.HEALTH ? c.HEA : c.Stats[fx.Stat];

            if(st.Modifiers.Contains(fx.Modifier))
                st.Modifiers.Remove(fx.Modifier);
        }
    }
}
