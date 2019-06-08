using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Zombie : Character
{

    public new void FixedUpdate()
    {
        base.FixedUpdate();

        if (!Alive() || !GameManager.Instance.GameStarted || !navMeshAgent.isOnNavMesh)
            return;

        //TODO: merge together with move's switch statement
        if (Attacking() && AttackTarget && AttackTarget.Alive() && InAttackRange()
        ) //has live enemy target and in attackrange
        {
            navMeshAgent.isStopped = true;

            if (_attackRoutine == null)
                _attackRoutine = StartCoroutine(AttackRoutine());
        }
        else
        {
            navMeshAgent.isStopped = false;
            SelectAction();
        }
    }

    private new void SelectAction()
    {
        switch (State)
        {
            case CharacterState.Idling:
                //reset morale
                Morale = COU.GetStatMax() * 2;

                if (actionInProgress)
                {
                    if (navMeshAgent.remainingDistance < 0.02f)
                        actionInProgress = false;
                }
                else if (IrritationMeter >= IrritaionTolerance)
                {
                    ChangeState(CharacterState.Attacking);
                }
                else if (Random.value < 0.015f) //selecting idle action
                {
                    actionInProgress = true;

                    Vector3 dest;

                    if (InArea)
                    {
                        if (GetClosestEnemy()
                            && //ANY friends fighting
                            InArea.PresentCharacters.Any(c => c.tag == tag && c.Alive() && c.Attacking())
                            )
                        {
                            ChangeState(CharacterState.Attacking, true);
                            Morale -= 5;
                            Target = GetClosestEnemy().transform.position;
                            dest = Target;
                        }
                        else if (tag == "Player")
                        {
                            if  (Team?.Leader.ClassType != Goblin.Class.Necromancer)
                            {
                                Debug.Log("Zombie becoming an enemy");
                                tag = "Enemy";
                                ChangeState(CharacterState.Attacking,true);
                                break;
                            }
                            else if (Team && (Team.Leader.Travelling() || Team.Leader.Fleeing()))
                            {
                                if (!TravellingToArea)
                                {
                                    ChangeState(CharacterState.Travelling);
                                    TravellingToArea = Team.Leader.TravellingToArea;
                                }
                                break;
                            }
                            dest = Team.Leader.transform.position;
                        }
                        else
                            dest = InArea.GetRandomPosInArea();
                    }
                    else
                    {
                        dest = transform.position + Random.insideUnitSphere * idleDistance;
                        dest.y = 0;
                    }

                    navMeshAgent.SetDestination(dest);//new Vector3(Random.Range(-idleDistance, idleDistance), 0,Random.Range(-idleDistance, idleDistance)));
                }
                break;
            case CharacterState.Attacking:
                if (AttackTarget && AttackTarget.Alive() && AttackTarget.InArea == InArea)
                {
                    if (AttackTarget.Fleeing())
                    {
                        var c = GetClosestEnemy();
                        if (c)
                            AttackTarget = c;
                    }

                    navMeshAgent.SetDestination(AttackTarget.transform.position);

                    //TODO: add random factor
                }
                else
                {
                    TargetGone();
                }
                break;
            case CharacterState.Travelling:
                navMeshAgent.SetDestination(Target);
                //check for arrival and stop travelling
                if (Vector3.Distance(transform.position, Target) < 3f)
                {
                    //Debug.Log(name +" arrived at target");
                    State = CharacterState.Idling;

                    actionInProgress = false;

                    break;
                }
                break;
            case CharacterState.Dead:
                break;
            case CharacterState.Surprised:
                navMeshAgent.isStopped = true;
                if (SurprisedTime + SurprisedStartTime <= Time.time)
                    ChangeState(CharacterState.Attacking);
                break;
            case CharacterState.Hiding:
            case CharacterState.Watching:
            case CharacterState.Fleeing:
            case CharacterState.Searching:
            case CharacterState.Provoking:
            case CharacterState.Resting:
                ChangeState(CharacterState.Idling, true);
                break;
            default:
                break;
        }

    }
}
