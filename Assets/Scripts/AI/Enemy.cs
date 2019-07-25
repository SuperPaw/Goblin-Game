using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
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
}
