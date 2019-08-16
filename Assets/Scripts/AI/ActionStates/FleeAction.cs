using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class FleeAction : ActionState
{
    private Coroutine ActionRoutine;
    public override Character.CharacterState StateType => Character.CharacterState.Fleeing;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        Area fleeingToArea = null;
        Debug.Log($"{ch.name}: Starting {StateType} action");

        while (ch.State == StateType)
        {
            yield return new WaitForFixedUpdate();

            (ch as Goblin)?.Speak(SoundBank.GoblinSound.PanicScream);

            if (fleeingToArea == ch.InArea && ch.navMeshAgent.remainingDistance < 1f)
            {
                //Debug.Log(name + "done fleeing");
                //reset morale
                ch.Morale = ch.COU.GetStatMax() * 2;
                ch.ChangeState(Character.CharacterState.Idling, true);
            }
            else if (fleeingToArea == null)// || (fleeingToArea &!navMeshAgent.hasPath))
            {
                //Select flee to area

                fleeingToArea = ch.TravellingToArea ? ch.TravellingToArea.GetClosestNeighbour(ch.transform.position, ch.StickToRoad, ch.TravellingToArea.ContainsRoads)
                    : ch.InArea.GetClosestNeighbour(ch.transform.position, ch.StickToRoad, ch.InArea.ContainsRoads);
                ch.navMeshAgent.SetDestination(fleeingToArea.GetRandomPosInArea());

                //Debug.Log(name + " fleeing to " +fleeingToArea);
                ch.TravellingToArea = fleeingToArea;

            }
            else if (!ch.NavigationPathIsStaleOrCompleted())
            {
                //Debug.Log(name + " updating fleeing to " + fleeingToArea);
                ch.navMeshAgent.SetDestination(fleeingToArea.GetRandomPosInArea());
            }
        }

        //TODO: handle cleanup
    }

    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}

}
