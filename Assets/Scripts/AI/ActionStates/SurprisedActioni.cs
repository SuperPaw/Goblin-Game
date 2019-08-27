using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class SurprisedAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Surprised;
    
    public override IEnumerator StateRoutine(Character g)
    {
        //Debug.Log($"{g.name}: Starting {StateType} action");
        
        g.navMeshAgent.isStopped = true;

        yield return new WaitForSeconds(g.SurprisedTime);

        g.ChangeState(Character.CharacterState.Attacking);
        

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
