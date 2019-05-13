using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WitchHut : PointOfInterest
{

    public void Attack(PlayerTeam team)
    {
        WitchHutView.CloseHut();
        StartCoroutine(Spawning(team));
    }

    public void PayToHeal(int i, PlayerTeam team)
    {
        team.Members.ForEach(g=>g.Heal());
        team.OnTreasureFound.Invoke(i);
        
    }

    public void BuyStaff(int amount, PlayerTeam team)
    {
        team.OnEquipmentFound.Invoke(EquipmentGen.GetEquipment(Equipment.EquipmentType.Stick),team.Leader);

        team.OnTreasureFound.Invoke(- amount);
    }

    public void BuySkull(int amount, PlayerTeam team)
    {
        team.OnEquipmentFound.Invoke(EquipmentGen.GetEquipment(Equipment.EquipmentType.Skull), team.Leader);

        team.OnTreasureFound.Invoke(-amount);
    }
}
