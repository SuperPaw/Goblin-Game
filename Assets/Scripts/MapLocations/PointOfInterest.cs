using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class PointOfInterest : MonoBehaviour
{
    public string AreaName;
    public string Inhabitants;
    public int MinSpawns = 1;
    public int MaxSpawns = 4;
    public Character MonsterPrefab;
    public Character NPCPrefab;
    public Area InArea;
    public bool HasBeenAttacked;
    public Sprite IconSprite;
    public PoiOptionController PoiOptionController;
    protected static PlayerTeam team;

    public List<OptionType> Options;
    public List<TradableItem> Tradables;

    [Serializable]
    public struct TradableItem
    {
        public bool BuyingThis;
        public Sprite Sprite;
        public Tradable Type;
        public int Price;
        public int AmountForSale;
    }

    public enum Tradable { Goblin, Food,Healing, Staff, SkullHat, Hat, Stick, Sword, Shoes, Armor,
        Gloves, Shirt, Cloth, Bow
    }

    public enum OptionType
    {
        BuyGoblin, SellGoblin, SacrificeGoblin, BuyStaff, BuyHat, StealTreasure, BuyFood, Healing, Attack, Explore, Lure, BuySlave,
    }


    //UI Player choices
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() { Action = null, Description = "No" };

    public Poi PoiType;

    public enum Poi { BigStone,Cave,Warrens,HumanFarm,HumanFort,Withchut,SlaveTrader,GoblinMerchant,Lake,ElvenTemple,
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
        //PoiOptionController.CreateOption(PointOfInterest.OptionType.Healing, Heal);
        //PoiOptionController.CreateOption(PointOfInterest.OptionType.BuyStaff, BuyStaff);
        //PoiOptionController.CreateOption(PointOfInterest.OptionType.BuyHat, BuyHat);

        foreach (var o in Options)
        {
            CreateOption(o);
        }
        foreach (var t in Tradables)
        {
            CreateOption(t);
        }
    }
    
    private void CreateOption(OptionType o)
    {
        switch (o)
        {
            case OptionType.SacrificeGoblin:
                break;
            case OptionType.StealTreasure:
                break;
            case OptionType.Attack:
                PoiOptionController.CreateOption(PointOfInterest.OptionType.Attack, () =>
                    PlayerChoice.CreateDoChoice(() => Attack(team), "Do you want to attack the witch"));
                break;
            case OptionType.Explore:
                break;
            case OptionType.Lure:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(o), o, null);
        }
    }
    private void CreateOption(TradableItem t)
    {
        switch (t.Type)
        {
            case Tradable.Goblin:
                break;
            case Tradable.Food:
                break;
            case Tradable.Healing:
                PoiOptionController.CreateOption(OptionType.Healing, ()=> Heal(t.Price));
                break;
            case Tradable.Staff:
                break;
            case Tradable.SkullHat:
                break;
            case Tradable.Hat:
                break;
            case Tradable.Stick:
                break;
            case Tradable.Sword:
                break;
            case Tradable.Shoes:
                break;
            case Tradable.Armor:
                break;
            case Tradable.Gloves:
                break;
            case Tradable.Shirt:
                break;
            case Tradable.Cloth:
                break;
            case Tradable.Bow:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Attack(PlayerTeam team)
    {
        StartCoroutine(Spawning(team));
    }

    public void PayToHeal(int i, PlayerTeam team)
    {
        team.Members.ForEach(g => g.Heal());
        team.OnTreasureFound.Invoke(-i);

    }


    private void Heal(int price)
    {
        if (team.Treasure >= price)
        {

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => PayToHeal(price,team), Description = "Ok" },
                    No
                },
                "Heal goblins for "+ price + " treasure?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have "+ price +" treasure to pay the "+ Inhabitants +".");
        }
    }

}
