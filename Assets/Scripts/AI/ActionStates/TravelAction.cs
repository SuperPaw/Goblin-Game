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
        Debug.Log($"{g.name}: Starting {StateType} action");

        g.navMeshAgent.SetDestination(g.Target);
        //check for arrival and stop travelling

        yield return new WaitUntil(() =>Vector3.Distance(g.transform.position, g.Target) < 3f);
        
        g.ChangeState(Character.CharacterState.Idling);
    }
    
    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}
    
}
