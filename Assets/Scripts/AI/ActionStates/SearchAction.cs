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
        Debug.Log($"{ch.name}: Starting {StateType} action");
        
        var g = ch as Goblin;

        if (!g)
        {
            Debug.LogError($"Non goblin: {StateType}");
        }

        if (g.LootTarget)
            g.navMeshAgent.SetDestination(g.LootTarget.transform.position);

        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (!g.LootTarget)
            {
                g.ChangeState(Character.CharacterState.Idling, true);
                break;
            }
        
            //if (!navMeshAgent.hasPath && !navMeshAgent.pathPending)
            //{
            //    Debug.Log($"{name}: Updating loot target path: {LootTarget}, {LootTarget.transform.position}");
            //    navMeshAgent.SetDestination(LootTarget.transform.position);
            //}

            //check for arrival and stop travelling
            if (!(Vector3.Distance(g.transform.position, g.LootTarget.transform.position) < 2f)) continue;

            if (g.LootTarget.ContainsLoot)
            {
                g.Speak(SoundBank.GoblinSound.Laugh);
                PopUpText.ShowText(g.name + " found " + g.LootTarget.Loot, g.transform);
                g.Team.OnTreasureFound.Invoke(1);
            }
            if (g.LootTarget.ContainsFood)
            {
                g.Speak(SoundBank.GoblinSound.Laugh);
                PopUpText.ShowText(g.name + " found " + g.LootTarget.Food, g.LootTarget.transform);
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

            g.ChangeState(Character.CharacterState.Idling, false, 5f);
            break;
        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
