using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

public class AttackAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Attacking;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Debug.Log($"{ch.name}: Starting {StateType} action");

        while (true)
        {
            if(!ch.Attacking())
                break;

            yield return new WaitForFixedUpdate();
            
            if (!ch.AttackTarget || !ch.AttackTarget.Alive() || ch.AttackTarget.InArea != ch.InArea)// || (ch.AttackTarget.Fleeing()&& ch.InArea.AnyEnemies())
            {
                //TODO. handle fleeing change S
                TargetGone(ch);
            }
            else if ( ch.InAttackRange()
            ) //has live enemy target and in attackrange
            {
                ch.attackAnimation = true;
                ch.transform.LookAt(ch.AttackTarget.transform);

                //should be tied to animation maybe?
                ch.navMeshAgent.isStopped = true;
            }
            else 
            {
                ch.attackAnimation = false;
                if(ch.navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
                    ch.navMeshAgent.SetDestination(ch.AttackTarget.transform.position);
            }
        }
        //TODO: handle cleanup
    }

    protected void TargetGone(Character ch)
    {
        if (!ch.Attacking()) return;

        var closest = ch.GetClosestEnemy();
        if (!closest)
        {
            ch.ChangeState(Character.CharacterState.Idling, true);
            ch.AttackTarget = null;
        }
        else
        {
            ch.AttackTarget = closest;

            //TODO: check what happens if character moves another place, before they get there
            ch.navMeshAgent.SetDestination(closest.transform.position);
        }
    }
    
    
    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}
    
}
