using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ProvokeAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Provoking;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Debug.Log($"{ch.name}: Starting {StateType} action");


        float ProvokeTime = 5;
        float ProvokeStartTime = Time.time;
        var g = ch as Goblin;
        var goingToProvoke = false;

        if (!g)
        {
            Debug.LogWarning("Non-goblin is being provocative");
            yield break;
        }


        if (!g.ProvokeTarget || g.ProvokeTarget.InArea != g.InArea)
        {
            g.Speak(SoundBank.GoblinSound.Laugh);
            g.ProvokeTarget = g.GetClosestEnemy();
            if(g.ProvokeTarget)
                g.navMeshAgent.SetDestination(g.ProvokeTarget.transform.position);
            goingToProvoke = true;
        }


        Vector3 provokeDest = g.ProvokeTarget.transform.position;

        while (ch.State == StateType)
        {
            yield return new WaitForFixedUpdate();

            //TODO: define these in the while statement instead
            if (!g.ProvokeTarget)
            {
                g.ChangeState(Character.CharacterState.Idling, true);
                break;
            }

            if (g.ProvokeTarget.Attacking())
            {
                g.ChangeState(Character.CharacterState.Attacking);
                break;
            }

            if (!goingToProvoke && g.navMeshAgent.remainingDistance < 0.5f)
            {
                goingToProvoke = true;

                provokeDest = g.ProvokeTarget.transform.position;
                g.navMeshAgent.SetDestination(provokeDest);

                ProvokeStartTime = Time.time;
            }

            //TODO: remove provoke start time and just use yield return waituntill
            if (ProvokeTime + ProvokeStartTime > Time.time)
            {
                //run away
                goingToProvoke = false;
                var dest = g.InArea.GetRandomPosInArea();
                g.navMeshAgent.SetDestination(dest);
                g.ProvokeTarget.IrritationMeter++;
            }
            if ((provokeDest - g.transform.position).magnitude < 4) //Provoke
            {
                g.navMeshAgent.isStopped = true;
                g.Speak(
                    PlayerController.GetDynamicReactions(PlayerController.DynamicState.Mocking));
            }
        }

        g.ChangeState(Character.CharacterState.Idling);

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
