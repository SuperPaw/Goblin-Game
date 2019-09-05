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
        //Debug.Log($"{ch.name}: Starting {StateType} action");

        var g = ch as Goblin;

        if (!g)
        {
            Debug.LogError($"Non goblin: {StateType}");
        }

        if (g.Team && g.Team.Challenger)
            g.navMeshAgent.SetDestination(g.Team.Challenger.transform.position);

        while (g.Team.Challenger && ch.GetState() == StateType)
        {
            yield return new WaitForFixedUpdate();

            if (!g.Team || !g.Team.Challenger)
            {
                //(ch as Goblin)?.Speak(SoundBank.GoblinSound.Laugh);
                g.ChangeState(Character.CharacterState.Idling, true);
                break;
            }
            else
            {
                if (Vector3.Distance(g.transform.position, g.Team.Challenger.transform.position) < 3f && g.Team.Challenger != g && g.Team.Leader != g)
                {
                    //cheer
                    g.Speak(PlayerController.GetDynamicReactions(PlayerController.DynamicState.ChiefBattleCheer));

                    g.navMeshAgent.ResetPath();
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
