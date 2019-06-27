using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PointOfInterest : MonoBehaviour
{
    public string AreaName;
    public string Inhabitants;
    public int MinSpawns = 1;
    public int MaxSpawns = 4;
    public Character MonsterPrefab;
    public Character NPCPrefab;
    public Area InArea;
    public bool HasBeenAttacked;
    //TODO: use this instead of the option
    public bool Attackable;
    public Sprite IconSprite;
    public PoiOptionController PoiOptionController;
    protected static PlayerTeam team;

    public List<OptionType> Options;
    public List<BuyableItem> Buyables;

    [Serializable]
    public class BuyableItem
    {
        public string Name;
        public Sprite Sprite;
        public Tradable Type;
        public Equipment.EquipmentType EquipmentType;
        public int Price;
        public int AmountForSale;
        public OptionType OptionType;
    }

    public enum Tradable { Goblin, Food,Healing, Equipment
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

        foreach (var o in Options)
        {
            CreateOption(o);
        }
        foreach (var t in Buyables.Where(b => b.AmountForSale > 0))
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
                    PlayerChoice.CreateDoChoice(() => Attack(team), "Do you want to attack the "+ Inhabitants));
                break;
            case OptionType.Explore:
                break;
            case OptionType.Lure:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(o), o, null);
        }
    }
    private void CreateOption(BuyableItem t)
    {
        switch (t.Type)
        {
            case Tradable.Goblin:
                break;
            case Tradable.Food:
                break;
            case Tradable.Healing:
                PoiOptionController.CreateOption(OptionType.Healing, ()=> HealBox(t.Price));
                break;
            case Tradable.Equipment:
                PoiOptionController.CreateOption(t.OptionType,()=> CreateBuyBox(t));
                break;
            default:
                throw new ArgumentOutOfRangeException(t.ToString());
        }
    }

    private void CreateBuyBox(BuyableItem b)
    {
        if (team.Treasure >= b.Price)
        {
            PlayerChoice.SetupPlayerChoice(new[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => Buy(b), Description = "Ok" },
                    No
                },
                $"Buy {b.Name} for {b.Price} treasure?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                $"You do not have {b.Price} treasure to buy {b.Name}.");
        }
    }

    private static void Buy(BuyableItem b)
    {
        //TODO: check that this works
        b.AmountForSale--;

        team.OnEquipmentFound.Invoke(EquipmentGen.GetEquipment(b.EquipmentType), team.Leader);

        team.OnTreasureFound.Invoke(-b.Price);
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

    private void HealBox(int price)
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
