using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class IdleAction : ActionState
{
    private Coroutine ActionRoutine;
    private bool actionInProgress;
    

    public override Character.CharacterState StateType => Character.CharacterState.Idling;
    
    public override IEnumerator StateRoutine(Character g)
    {
        Debug.Log($"{g.name}: Starting {StateType} action");

        while (g.Alive())
        {
            yield return new WaitForFixedUpdate();

            if (actionInProgress)
            {
                if (g.navMeshAgent.remainingDistance < 0.02f)
                    actionInProgress = false;
            }
            else if (g.IrritationMeter >= g.IrritaionTolerance)
            {
                g.ChangeState(Character.CharacterState.Attacking, true);
                break;
            }
            else if (Random.value < 0.015f) //selecting idle action
            {
                if (!g.IsChief())
                    (g as Goblin)?.Speak(PlayerController.GetDynamicReactions(PlayerController.DynamicState.Idle));

                actionInProgress = true;

                Vector3 dest;

                if (g.InArea)
                {
                    if (g.GetClosestEnemy()
                        && ( //ANY friends fighting
                        g.InArea.PresentCharacters.Any(c => c.tag == g.tag && c.Alive() && c.Attacking())
                        // I am aggressive wanderer
                        || (g.StickToRoad && g.InArea.PresentCharacters.Any(c => c.tag == "Player" & !c.Hiding()))
                        ))
                    {
                        //Debug.Log(name + ": Joining attack without beeing attacked");

                        g.ChangeState(Character.CharacterState.Attacking, true);
                        g.Morale -= 5;
                        break;
                    }
                    else if ((g as Goblin) && g.Team && g.Team.Leader.InArea != g.InArea & !g.Team.Leader.Travelling())
                    {
                        g.TravellingToArea = g.Team.Leader.InArea;
                        dest = g.Team.Leader.InArea.GetRandomPosInArea();
                    }
                    else if ((g as Goblin) && g.tag == "Player" && g.GetClosestEnemy() && (g.GetClosestEnemy().transform.position - g.transform.position).magnitude < g.provokeDistance)
                    {
                        g.ChangeState(Character.CharacterState.Provoking, true);
                        var ctx = g.GetClosestEnemy();
                        (g as Goblin).ProvokeTarget = ctx;
                        (g as Goblin)?.Speak(PlayerController.GetDynamicReactions(PlayerController.DynamicState.Mocking));
                        dest = ctx.transform.position;
                        break;
                    }
                    else if (g.StickToRoad)
                    {
                        var goingTo = g.InArea.GetClosestNeighbour(g.transform.position, true);

                        //TODO: handle this in moveto method instead
                        dest = goingTo.PointOfInterest ? goingTo.GetRandomPosInArea() : goingTo.transform.position;

                        //Debug.Log(name + ": Wandering to "+ goingTo);
                        g.Target = dest;
                        g.TravellingToArea = goingTo;

                        goingTo.PresentCharacters.ForEach(c => c.SpotArrival(g));

                        g.ChangeState(Character.CharacterState.Travelling, true);
                        break;
                    }
                    else
                        dest = g.InArea.GetRandomPosInArea();
                }
                else
                {
                    dest = g.transform.position + Random.insideUnitSphere * g.idleDistance;
                    dest.y = 0;
                }

                g.navMeshAgent.SetDestination(dest);//new Vector3(Random.Range(-idleDistance, idleDistance), 0,Random.Range(-idleDistance, idleDistance)));

            }
            //TODO: use a different method for activity selection than else if
            else if (Random.value < 0.0025f && g as Goblin && g.Team
                && !(g.Team.Leader == g) && g.Team.Members.Count > 4 && (g as Goblin).ClassType > Goblin.Class.Slave
                & !g.Team.Challenger && (g.Team.Leader as Goblin).CurrentLevel < ((Goblin)g).CurrentLevel)
            {
                //TODO: make it only appear after a while

                Debug.Log("Chief Fight!!");
                g.Team.ChallengeForLeadership(g as Goblin);
            }
            //TODO: define better which characters should search stuff
            else if (Random.value < 0.001f * g.SMA.GetStatMax() && g.Team && g as Goblin && !g.InArea.AnyEnemies() && g.InArea.Lootables.Any(l => !l.Searched))
            {
                var loots = g.InArea.Lootables.Where(l => !l.Searched).ToArray();

                var loot = loots[Random.Range(0, loots.Count())];

                (g as Goblin).Search(loot);
            }
        }


        //TODO: clean up?
    }
    
    //public override void EndState()
    //{
    //    throw new System.NotImplementedException();
    //}
    
}
