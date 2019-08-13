using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class WatchingAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Watching;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Debug.Log($"{ch.name}: Starting {StateType} action");

        if (ch.Team && ch.Team.Challenger)
            ch.navMeshAgent.SetDestination(ch.Team.Challenger.transform.position);

        while (ch.Team.Challenger)
        {
            yield return new WaitForFixedUpdate();

            if (!ch.Team || !ch.Team.Challenger)
            {
                //(ch as Goblin)?.Speak(SoundBank.GoblinSound.Laugh);
                ch.ChangeState(Character.CharacterState.Idling, true);
                break;
            }
            else
            {
                if (Vector3.Distance(ch.transform.position, ch.Team.Challenger.transform.position) < 3f && ch.Team.Challenger != ch && ch.Team.Leader != ch)
                {
                    //cheer
                    (ch as Goblin)?.Speak(PlayerController.GetDynamicReactions(PlayerController.DynamicState.ChiefBattleCheer));

                    ch.navMeshAgent.ResetPath();
                }
            }
        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
