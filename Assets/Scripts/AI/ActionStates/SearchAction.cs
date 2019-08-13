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

        if(ch.LootTarget)
            ch.navMeshAgent.SetDestination(ch.LootTarget.transform.position);

        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (!ch.LootTarget)
            {
                ch.ChangeState(Character.CharacterState.Idling, true);
                break;
            }
        
            //if (!navMeshAgent.hasPath && !navMeshAgent.pathPending)
            //{
            //    Debug.Log($"{name}: Updating loot target path: {LootTarget}, {LootTarget.transform.position}");
            //    navMeshAgent.SetDestination(LootTarget.transform.position);
            //}

            //check for arrival and stop travelling
            if (!(Vector3.Distance(ch.transform.position, ch.LootTarget.transform.position) < 2f)) continue;

            if (ch.LootTarget.ContainsLoot)
            {
                (ch as Goblin)?.Speak(SoundBank.GoblinSound.Laugh);
                PopUpText.ShowText(ch.name + " found " + ch.LootTarget.Loot, ch.LootTarget.transform);
                ch.Team.OnTreasureFound.Invoke(1);
            }
            if (ch.LootTarget.ContainsFood)
            {
                (ch as Goblin)?.Speak(SoundBank.GoblinSound.Laugh);
                PopUpText.ShowText(ch.name + " found " + ch.LootTarget.Food, ch.LootTarget.transform);
                ch.Team.OnFoodFound.Invoke(5);
            }
            if (ch as Goblin)
                foreach (var equipment in ch.LootTarget.EquipmentLoot)
                {
                    //TODO: create player choice for selecting goblin
                    (ch as Goblin).Speak(SoundBank.GoblinSound.Laugh);

                    ch.Team?.OnEquipmentFound.Invoke(equipment, ch as Goblin);
                }

            ch.LootTarget.EquipmentLoot.Clear();

            ch.LootTarget.ContainsFood = false;
            ch.LootTarget.ContainsLoot = false;
            ch.LootTarget.Searched = true;

            ch.ChangeState(Character.CharacterState.Idling, false, 5f);
            break;
        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
