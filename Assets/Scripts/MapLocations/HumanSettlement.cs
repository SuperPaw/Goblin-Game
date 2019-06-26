using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanSettlement : PointOfInterest
{
    public string Name;
    public GameObject[] SpawnEnemies;
    public bool WinCondition;
    public int InitialEnemies;
    public int EnemiesOnAttack;

    public void AttackSettlement()
    {
        StartCoroutine(Spawning());
    }

    //TODO: move to general POI method
    private IEnumerator Spawning()
    {
        if (HasBeenAttacked)
            yield break;

        HasBeenAttacked = true;

        yield return new WaitForSeconds(0.5f);
        
        var spawn = new List<Character>();

        for (int i = 0; i < EnemiesOnAttack; i++)
        {
            var z = MapGenerator.GenerateCharacter(SpawnEnemies[Random.Range(0,SpawnEnemies.Length)], InArea, NpcHolder.Instance.transform,true).GetComponent<Character>();

            z.ChangeState(Character.CharacterState.Attacking);

            spawn.Add(z);

            yield return new WaitForSeconds(Random.Range(0, 1.5f));
        }

        if (WinCondition)
        {
            yield return new WaitUntil(() => spawn.All(s => !s.Alive()));

            Debug.Log("PLAYER HAS WON");

            PopUpText.ShowText("The goblins have found a new home!");
            GameManager.GameOver(true);
        }
    }

    public override void SetupMenuOptions()
    {
        PoiOptionController.CreateOption(PointOfInterest.OptionType.Attack,() =>
            PlayerChoice.CreateDoChoice(AttackSettlement, "Do you want to attack the Human " + Name));
    }


}
