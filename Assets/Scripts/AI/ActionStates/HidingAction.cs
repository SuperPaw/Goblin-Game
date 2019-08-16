using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class HidingAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Hiding;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Debug.Log($"{ch.name}: Starting {StateType} action");

        ch.navMeshAgent.SetDestination(ch.hiding.HideLocation.transform.position);

        while (ch.State == StateType)
        {
            yield return new WaitForFixedUpdate();

            if (!ch.Hidingplace)
            {
                ch.ChangeState(Character.CharacterState.Idling,true);
                break;
            }
            //already set the destination
            if ((ch.navMeshAgent.destination - ch.hiding.HideLocation.transform.position).sqrMagnitude < 1f)
            {
                if (ch.InArea.AnyEnemies() && ch.navMeshAgent.remainingDistance > 0.2f)
                {
                    ch.ChangeState(Character.CharacterState.Idling);
                    break;
                }
            }
            //else
            //{
            //    navMeshAgent.SetDestination(hiding.HideLocation.transform.position);
            //}
        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
