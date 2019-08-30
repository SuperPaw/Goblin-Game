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
        //Debug.Log($"{ch.name}: Starting {StateType} action");

        while (ch.State == StateType)
        {
            yield return new WaitForFixedUpdate();
            
            if (!ch.AttackTarget || !ch.AttackTarget.Alive() || ch.AttackTarget.InArea != ch.InArea)// || (ch.AttackTarget.Fleeing()&& ch.InArea.AnyEnemies())
            {
                //TODO. handle fleeing change S
                Debug.Log($"{ch}'s Target is gone");
                TargetGone(ch);
            }
            else if ( ch.InAttackRange()) //has live enemy target and in attackrange
            {
                ch.attackAnimation = true;
                ch.transform.LookAt(ch.AttackTarget.transform);

                //should be tied to animation maybe?
                ch.navMeshAgent.isStopped = true;
            }
            else 
            {
                ch.attackAnimation = false;
                ch.navMeshAgent.isStopped = false;

                if (ch.NavigationPathIsStaleOrCompleted())
                {
                    Debug.Log($"{ch} going to attack target {ch.AttackTarget}");
                    ch.navMeshAgent.SetDestination(ch.AttackTarget.transform.position);
                }
            }
        }
        ch.navMeshAgent.isStopped = false;

        //TODO: handle cleanup
    }

    protected void TargetGone(Character ch)
    {
        var closest = ch.GetClosestEnemy();
        if (!closest || !ch.Attacking())
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
