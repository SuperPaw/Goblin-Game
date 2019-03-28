using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monument : PointOfInterest
{

    public Character ZombiePrefab;
    public int Treasure = 3;

    public void SpawnDead(PlayerTeam team)
    {
        StartCoroutine(Spawning(team));
    }

    private IEnumerator Spawning(PlayerTeam team)
    {
        yield return new WaitForSeconds(1.5f);

        Sun.Night();

        var zs = Random.Range(5, 10);

        for (int i = 0; i < zs; i++)
        {
            var z = MapGenerator.GenerateCharacter(ZombiePrefab.gameObject, InArea, NpcHolder.Instance.transform,true).GetComponent<Character>();

            z.ChangeState(Character.CharacterState.Attacking);

            team.Members.ForEach(m=> m.Morale -= 1);

            yield return new WaitForSeconds(Random.Range(0,1.5f));
        }
    }
}
