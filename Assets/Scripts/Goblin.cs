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

    new void Start()
    {
        base.Start();

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

        // -------------- CHECK FOR LEADER DISTANCE AND MOVE TO HIM -----------------
        if (Team.Leader != this &! Attacking())
        {
            
        }

    }
}
