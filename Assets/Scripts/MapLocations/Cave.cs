using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cave : PointOfInterest
{
    public int Difficulty;
    public int Prize;
    public bool Explored;

    public void LureMonster(PlayerTeam team, int food)
    {
        team.Food -= food;

        CaveView.CloseCave();

        StartCoroutine(Spawning(team));
    }


    public void SendInGoblin(Goblin g)
    {
        StartCoroutine(SendGoblinInCaveRoutine(g));
    }

    private IEnumerator SendGoblinInCaveRoutine(Goblin g)
    {
        CaveView.CloseCave();

        //Goblin walk there
        g.MoveTo(transform.position);

        //Wait for resolution
        yield return new WaitForSeconds(2);
        
        //turn off goblin
        g.gameObject.SetActive(false);

        //Wait for resolution
        yield return new WaitForSeconds(2);

        //Arrive back with treasure or Remove goblin
        if (Random.Range(0, g.SMA.GetStatMax()) >= Difficulty)
        {
            g.Xp += 10;
            g.Team.Treasure += Prize;
            PopUpText.ShowText(g.name + " found many goblin treasures in Cave!");
            g.gameObject.SetActive(true);
            g.ChangeState(Character.CharacterState.Idling,true);

            Explored = true;
        }
        else
        {
            PopUpText.ShowText(g + " did not return from exploring the cave!");
            g.Health = 0;
            g.Team.Members.Remove(g);
        }
    }
}
