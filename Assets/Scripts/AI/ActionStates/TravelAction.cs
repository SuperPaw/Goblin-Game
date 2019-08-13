using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class TravelAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Travelling;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Debug.Log($"{ch.name}: Starting {StateType} action");

        ch.navMeshAgent.SetDestination(ch.Target);
        //check for arrival and stop travelling

        while (Vector3.Distance(ch.transform.position, ch.Target) < 3f)
        {
            yield return new WaitForFixedUpdate();
        }

        ch.ChangeState(Character.CharacterState.Idling);
    }
    
    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}
    
}
