using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class IdleAction : ActionState
{
    private Coroutine ActionRoutine;
    private bool actionInProgress;
    private float IdleMin = 1f;
    private float IdleMax = 10f;
    

    public override Character.CharacterState StateType => Character.CharacterState.Idling;
    
    public override IEnumerator StateRoutine(Character ch)
    {
        //Debug.Log($"{ch.name}: Starting {StateType} action");

        yield return new WaitUntil(() => GameManager.Instance.GameStarted);

        var g = ch as Goblin;

        while (ch.GetState() == StateType)
        {
            yield return new WaitForFixedUpdate();

            if (actionInProgress)
            {
                if (ch.navMeshAgent.remainingDistance < 0.02f)
                    actionInProgress = false;
            }
            else if (ch.IrritationMeter >= ch.IrritaionTolerance)
            {
                ch.ChangeState(Character.CharacterState.Attacking, true);
                ch.IrritationMeter = 0;

                break;
            }

            yield return new WaitForSeconds(Random.Range(IdleMin, IdleMax));

            if (!ch.Idling()) break;

            //Chief challenge?
            if ( g && (g).Mood < Goblin.Happiness.Neutral && ch.Team
                && !(ch.Team.Leader == ch) && ch.Team.Members.Count > 4 && (g).ClassType > Goblin.Class.Slave
                & !ch.Team.Challenger && (ch.Team.Leader as Goblin).CurrentLevel <= ((Goblin)ch).CurrentLevel)
            {
                //TODO: make it only appear after a while

                Debug.Log("Chief Fight!!");
                ch.Team.ChallengeForLeadership(g);
                continue;
            }
            
            //search stuff?
            if (Random.value < 0.025f * ch.SMA.GetStatMax() && ch.Team && g && !ch.InArea.AnyEnemies() && ch.InArea.Lootables.Any(l => !l.Searched))
            {
                var loots = ch.InArea.Lootables.Where(l => !l.Searched).ToArray();

                var loot = loots[Random.Range(0, loots.Count())];

                (g).Search(loot);
                continue;
            }

            //otherwise just walk someplace at random

            if (!ch.IsChief() && g)
                g.Speak(PlayerController.GetMoodReaction(g.Mood));

            actionInProgress = true;

            Vector3 dest;

            if (ch.InArea)
            {
                if (ch.GetClosestEnemy()
                    && ( //ANY friends fighting
                    ch.InArea.PresentCharacters.Any(c => c.tag == ch.tag && c.Alive() && c.Attacking())
                    // I am aggressive wanderer
                    || (ch.StickToRoad && ch.InArea.PresentCharacters.Any(c => c.tag == "Player" &&c.Alive() & !c.Hiding()))
                    ))
                {
                    //Debug.Log(name + ": Joining attack without beeing attacked");

                    ch.ChangeState(Character.CharacterState.Attacking, true);
                    ch.Morale -= 5;
                    break;
                }
                else if ((g) && ch.Team && ch.Team.Leader.InArea != ch.InArea & !ch.Team.Leader.Travelling())
                {
                    ch.TravellingToArea = ch.Team.Leader.InArea;
                    dest = ch.Team.Leader.InArea.GetRandomPosInArea();
                }
                else if ((g) && ch.tag == "Player" && ch.GetClosestEnemy() && (ch.GetClosestEnemy().transform.position - ch.transform.position).magnitude < ch.provokeDistance)
                {
                    ch.ChangeState(Character.CharacterState.Provoking, true);
                    var ctx = ch.GetClosestEnemy();
                    (g).ProvokeTarget = ctx;
                    (g)?.Speak(PlayerController.GetDynamicReactions(PlayerController.DynamicState.Mocking));
                    dest = ctx.transform.position;
                    break;
                }
                else if (ch.StickToRoad)
                {
                    var goingTo = ch.InArea.GetClosestNeighbour(ch.transform.position, true);

                    //TODO: handle this in moveto method instead
                    dest = goingTo.PointOfInterest ? goingTo.GetRandomPosInArea() : goingTo.transform.position;

                    //Debug.Log(name + ": Wandering to "+ goingTo);
                    ch.Target = dest;
                    ch.TravellingToArea = goingTo;

                    goingTo.PresentCharacters.ForEach(c => c.SpotArrival(ch));

                    ch.ChangeState(Character.CharacterState.Travelling, true);
                    break;
                }
                else
                    dest = ch.InArea.GetRandomPosInArea();
            }
            else
            {
                dest = ch.transform.position + Random.insideUnitSphere * ch.idleDistance;
                dest.y = 0;
            }

            ch.navMeshAgent.SetDestination(dest);//new Vector3(Random.Range(-idleDistance, idleDistance), 0,Random.Range(-idleDistance, idleDistance)));

            
        }
    }
}
