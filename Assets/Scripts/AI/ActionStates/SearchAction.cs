using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class SearchAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Searching;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        //Debug.Log($"{ch.name}: Starting {StateType} action");
        
        var g = ch as Goblin;

        if (!g)
        {
            Debug.LogError($"Non goblin: {StateType}");
        }

        if (g.LootTarget)
            g.navMeshAgent.SetDestination(g.LootTarget.transform.position);

        while (ch.GetState() == StateType)
        {
            yield return new WaitForFixedUpdate();

            if (!g.LootTarget)
            {
                g.ChangeState(Character.CharacterState.Idling, true);
                break;
            }

            if (ch.NavigationPathIsStaleOrCompleted())
            {
                Debug.Log($"{ch}: Updating loot target path: {ch.LootTarget}, {ch.LootTarget.transform.position}");
                ch.navMeshAgent.SetDestination(ch.LootTarget.transform.position);
            }

            //check for arrival and stop travelling
            if ((Vector3.Distance(g.transform.position, g.LootTarget.transform.position) > 1.5f)) continue;

            g.navMeshAgent.ResetPath();

            if (g.LootTarget.ContainsLoot)
            {
                g.Speak(SoundBank.GoblinSound.Laugh);
                PopUpText.ShowText(g.name + " found " + g.LootTarget.Loot.Name, g.transform, g.LootTarget.Loot.LootImage);
                g.Team.OnTreasureFound.Invoke(1);
            }
            if (g.LootTarget.ContainsFood)
            {
                g.Speak(SoundBank.GoblinSound.Laugh);
                PopUpText.ShowText(g.name + " found " + g.LootTarget.Food.Name, g.transform,g.LootTarget.Food.LootImage);
                g.Team.OnFoodFound.Invoke(5);
            }

            foreach (var equipment in g.LootTarget.EquipmentLoot)
            {
                //TODO: create player choice for selecting goblin
                g.Speak(SoundBank.GoblinSound.Laugh);

                g.Team?.OnEquipmentFound.Invoke(equipment, g);
            }

            g.LootTarget.EquipmentLoot.Clear();

            g.LootTarget.ContainsFood = false;
            g.LootTarget.ContainsLoot = false;
            g.LootTarget.Searched = true;

            g.ChangeState(Character.CharacterState.Idling, false, 4f);
            break;
        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
