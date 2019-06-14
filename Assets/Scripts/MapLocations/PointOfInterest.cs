using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class PointOfInterest : MonoBehaviour
{
    public string AreaName;
    public int MinSpawns = 1;
    public int MaxSpawns = 4;
    public Character MonsterPrefab;
    public Area InArea;
    public bool HasBeenAttacked;
    public Sprite IconSprite;
    public PoiOptionController PoiOptionController;
    protected static PlayerTeam team;

    //UI Player choices
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() { Action = null, Description = "No" };

    public Poi PoiType;

    public enum Poi { BigStone,Cave,Warrens,HumanFarm,HumanFort,Withchut,
        Count }

    void Awake()
    {
        PoiOptionController = GetComponentInChildren<PoiOptionController>();
        if(!team) team = FindObjectOfType<PlayerTeam>();
    }

    public IEnumerator Spawning(PlayerTeam team)
    {
        HasBeenAttacked = true;

        yield return new WaitForSeconds(1.5f);

        var zs = Random.Range(MinSpawns, MaxSpawns);

        for (int i = 0; i < zs; i++)
        {
            var enm = MapGenerator.GenerateCharacter(MonsterPrefab.gameObject, InArea, NpcHolder.Instance.transform,true).GetComponent<Character>();

            enm.ChangeState(Character.CharacterState.Attacking);

            yield return new WaitForSeconds(Random.Range(0, 1.5f));
        }
    }

    public virtual void SetupMenuOptions()
    {
        Debug.LogError("Virtual method called!");
    }
}
