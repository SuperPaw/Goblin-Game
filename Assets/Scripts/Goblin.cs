using System.Collections;
    using System.Collections.Generic;
    using UnityEditor.Experimental.UIElements;
    using UnityEngine;
    using UnityEngine.Assertions.Must;
    using UnityEngine.Events;

public class Goblin : Character
{
    [Header("Goblin Specific")]
    //consider removing to Character
    //meaning how often they notice what other are doing
    //0-10
    public int Awareness;
    private int goToLeaderDistance = 3;

    public enum Class
    {
        NoClass, Swarmer, Shooter, Ambusher, Scout, Slave
    }

    public Class ClassType;
        

    new void Start()
    {
        base.Start();

        Awareness = ATT.GetStatMax();

        StartCoroutine(AwarenessLoop());
    }
    
    private IEnumerator AwarenessLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(1, 10 - Awareness));

            CheckWhatOthersAreDoing();
        }
    }

    private void CheckWhatOthersAreDoing()
    {
        if (!Team)
            return;
        
        //these could also all be more random dependant on Awareness

        //Debug.Log("checking leader distance ");

        // -------------- CHECK FOR LEADER DISTANCE AND MOVE TO HIM -----------------
        //TODO: make a readyForOrder() method, instead of checking on each state every time
        if (Team.Leader != this & !Attacking() & !Fleeing() &! Travelling() &&
            (transform.position - Team.Leader.transform.position).magnitude > goToLeaderDistance)
        {
            //Debug.Log(name + " going to leader");
            //TODO: make it disappear from at a certain range. Lost goblin...
            var newDes = Team.Leader.transform.position + new Vector3(Random.Range(-0.1f,0.1f), 0, Random.Range(-0.1f,0.1f));


            navMeshAgent.SetDestination(Team.Leader.transform.position);

        }

    }
}
