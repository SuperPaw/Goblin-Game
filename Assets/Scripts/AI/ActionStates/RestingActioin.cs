using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class RestingAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Resting;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Debug.Log($"{ch.name}: Starting {StateType} action");

        if (!ch.Team || !ch.Team.Campfire)
        {
            ch.ChangeState(Character.CharacterState.Idling, true);
            yield break;
        }

        ch.navMeshAgent.SetDestination(ch.Team.Campfire.transform.position);

        yield return new WaitUntil(()=> !ch.Team.Campfire || Vector3.Distance(ch.transform.position, ch.Team.Campfire.transform.position) < 4f);

        ch.navMeshAgent.isStopped = true;

        while (ch.Team.Campfire)
        {
            yield return new WaitForFixedUpdate();

            (ch as Goblin)?.Speak(SoundBank.GoblinSound.Eat);

        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
