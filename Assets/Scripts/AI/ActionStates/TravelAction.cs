using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class TravelAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Travelling;
    
    public override IEnumerator StateRoutine(Character g)
    {
        //Debug.Log($"{g.name}: Starting {StateType} action");

        g.navMeshAgent.SetDestination(g.Target);
        //check for arrival and stop travelling

        yield return new WaitUntil(() => g.GetState() != Character.CharacterState.Travelling ||Vector3.Distance(g.transform.position, g.Target) < 3f);
        
        if(g.GetState() == Character.CharacterState.Travelling)
            g.ChangeState(Character.CharacterState.Idling);
    }
        
}
