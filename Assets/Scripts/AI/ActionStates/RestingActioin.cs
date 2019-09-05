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
        //Debug.Log($"{ch.name}: Starting {StateType} action");

        var g = ch as Goblin;

        if (!g)
        {
            Debug.LogError($"Non goblin: {StateType}");
        }

        if (!g.Team || !g.Team.Campfire)
        {
            g.ChangeState(Character.CharacterState.Idling, true);
            yield break;
        }

        g.navMeshAgent.SetDestination(g.Team.Campfire.transform.position);

        yield return new WaitUntil(()=> !g.Team.Campfire || Vector3.Distance(g.transform.position, g.Team.Campfire.transform.position) < 4f);

        g.OnMoodChange.Invoke(3);

        g.navMeshAgent.isStopped = true;

        while (g.Team.Campfire)
        {
            yield return new WaitForFixedUpdate();

            g.Speak(SoundBank.GoblinSound.Eat);

        }
        ch.ChangeState(Character.CharacterState.Idling);

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
