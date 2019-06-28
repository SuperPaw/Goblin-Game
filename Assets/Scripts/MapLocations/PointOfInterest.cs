using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PointOfInterest : MonoBehaviour
{
    [Header("Info")]
    public string AreaName;
    public string Inhabitants;
    public Poi PoiType;
    public Area InArea;
    public Character NPCPrefab;
    public int NoOfNpcs;
    public Sprite IconSprite;
    public PoiOptionController PoiOptionController;
    protected static PlayerTeam team;
    public List<Character> Members = new List<Character>();

    [Header("Attack stats")]
    public Character MonsterPrefab;
    public int MinSpawns = 0;
    public int MaxSpawns = 0;
    public bool WinCondition;
    public int InitialEnemies;
    public bool HasBeenAttacked;
    //TODO: use this instead of the option
    [HideInInspector]
    public bool Attackable;
    public bool LivingDead;

    [Header("POI Options")]
    public List<OptionType> Options;
    public List<BuyableItem> Buyables;
    public Goblin GoblinPrefab;

    [Serializable]
    public class BuyableItem
    {
        public string Name;
        public Sprite Sprite;
        public Tradable Type;
        public Goblin.Class BuyableClass;
        public Equipment.EquipmentType EquipmentType;
        public int Price;
        public int AmountForSale;
        public OptionType OptionType;
    }

    public enum Tradable { Goblin, Food, Healing, Equipment
    }

    public enum OptionType
    {
        BuyGoblin, SellGoblin, SacrificeGoblin, BuyStaff, BuyHat, StealTreasure, BuyFood, Healing, Attack, Explore, Lure, BuySlave,
    }

    [Header("Exploration stats")]
    public bool Explored;
    public int Difficulty;
    //TODO: replace with types of rewards
    public int Treasure;
    

    //UI Player choices
    public PlayerChoice.ChoiceOption OkOption = new PlayerChoice.ChoiceOption() { Action = null, Description = "Ok" };
    public PlayerChoice.ChoiceOption No = new PlayerChoice.ChoiceOption() { Action = null, Description = "No" };


    public enum Poi { BigStone,Cave,Warrens,HumanFarm,HumanFort,Withchut,SlaveTrader,GoblinMerchant,Lake,ElvenTemple,
        Count }

    void Awake()
    {
        PoiOptionController = GetComponentInChildren<PoiOptionController>();
        if(!team) team = FindObjectOfType<PlayerTeam>();
    }

    void Start()
    {
        //TODO: redudant differentation
        for (int j = 0; j < InitialEnemies; j++)
        {
            var o = MapGenerator.GenerateCharacter(MonsterPrefab.gameObject, InArea, InArea.transform);
            var g = o.GetComponent<Character>();
            g.tag = "Enemy";
            Members.Add(g);

        }
        for (int j = 0; j < NoOfNpcs; j++)
        {
            if(!NPCPrefab)
                Debug.LogWarning(AreaName + " does not have npc set");

            var o = MapGenerator.GenerateCharacter(NPCPrefab.gameObject, InArea, InArea.transform);
            var g = o.GetComponent<Character>();
            g.tag = "NPC";
            Members.Add(g);
        }
    }

    protected IEnumerator Spawning(PlayerTeam team, bool livingDead = false)
    {
        HasBeenAttacked = true;

        yield return new WaitForSeconds(1.5f);

        foreach (var m in Members.Where(mem => mem.InArea == InArea))
        {
            m.tag = "Enemy";
            m.ChangeState(Character.CharacterState.Attacking);
        }

        if(livingDead) Sun.Night();

        var zs = Random.Range(MinSpawns, MaxSpawns);

        var spawn = new List<Character>();

        if (MonsterPrefab)
        {
            for (int i = 0; i < zs; i++)
            {
                var enm = MapGenerator
                    .GenerateCharacter(MonsterPrefab.gameObject, InArea, NpcHolder.Instance.transform, true)
                    .GetComponent<Character>();

                enm.tag = "Enemy";

                enm.ChangeState(Character.CharacterState.Attacking);

                if (livingDead)
                    team.Members.ForEach(m => m.Morale -= 1);

                spawn.Add(enm);

                yield return new WaitForSeconds(Random.Range(0, 1.5f));
            }
        }

        if (WinCondition)
        {
            yield return new WaitUntil(() => spawn.All(s => !s.Alive()));

            Debug.Log("PLAYER HAS WON");

            PopUpText.ShowText("The goblins have found a new home!");
            GameManager.GameOver(true);
        }

        spawn.Clear();
    }

    public virtual void SetupMenuOptions()
    {
        //Debug.LogError("Virtual method called!");
        if (HasBeenAttacked)
            return;

        foreach (var o in Options)
        {
            CreateOption(o);
        }

        foreach (var t in Buyables.Where(b => b.AmountForSale > 0))
        {
            PoiOptionController.CreateOption(t.OptionType, () => CreateBuyBox(t));
        }
    }
    
    private void CreateOption(OptionType o)
    {
        switch (o)
        {
            case OptionType.SellGoblin:
                PoiOptionController.CreateOption(OptionType.SellGoblin,SellGoblinBox);
                break;
            case OptionType.SacrificeGoblin:
                PoiOptionController.CreateOption(OptionType.SacrificeGoblin, SacGoblinBox);
                break;
            case OptionType.StealTreasure:
                PoiOptionController.CreateOption(OptionType.StealTreasure, StealTreasureBox);
                break;
            case OptionType.Attack:
                PoiOptionController.CreateOption(OptionType.Attack, () =>
                    PlayerChoice.CreateDoChoice(() => Attack(team), "Do you want to attack the "+ Inhabitants));
                break;
            case OptionType.Explore:
                PoiOptionController.CreateOption(OptionType.Explore, SendInGoblinBox);
                break;
            case OptionType.Lure:
                PoiOptionController.CreateOption(OptionType.Lure,LureMonsterBox);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(o), o, null);
        }
    }

    void SendInGoblinBox()
    {
        if (!Explored)
        {
            var options = team.Members.Where(ge => ge.InArea == InArea).OrderByDescending(g=>g.SMA.GetStatMax()).Take(3).Select(g =>
                new PlayerChoice.ChoiceOption()
                {
                    Action = () => SendInGoblin(g),
                    Description = g.name + ", " + g.SMA.GetStatMax() + " smarts"
                }).ToList();
            options.Add(No);

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                $"Send a Goblin to explore the {AreaName}?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                $"You have already explored the {AreaName}.");
        }
    }
    
    private void SendInGoblin(Goblin g)
    {
        StartCoroutine(ExploreRoutine(g));
    }

    private IEnumerator ExploreRoutine(Goblin g)
    {
        //Goblin walk there
        g.MoveTo(transform.position);

        //Wait for resolution
        yield return new WaitForSeconds(2);

        //turn off goblin
        g.gameObject.SetActive(false);

        //Wait for resolution
        yield return new WaitForSeconds(2);

        //Arrive back with treasure or Remove goblin
        if (Random.Range(1, g.SMA.GetStatMax()+1) >= Difficulty)
        {
            g.Xp += 10;
            g.Team.OnTreasureFound.Invoke(Treasure);
            PopUpText.ShowText($"{g.name} found {Treasure} goblin treasures in {AreaName}!");
            g.gameObject.SetActive(true);
            g.ChangeState(Character.CharacterState.Idling, true);

            Explored = true;
        }
        else
        {
            PopUpText.ShowText($"{g.name} did not return from exploring the {AreaName}!");
            g.Health = 0;
            g.Team.Members.Remove(g);
        }
    }
    
    private void Attack(PlayerTeam team)
    {
        StartCoroutine(Spawning(team));
    }

    public void PayToHeal(int i)
    {
        team.Members.ForEach(g => g.Heal());
        team.OnTreasureFound.Invoke(-i);
    }

    void LureMonsterBox()
    {
        if (team.Food >= 1)
        {
            var amount = Mathf.Min(team.Food, 3);

            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[]
                {
                    new PlayerChoice.ChoiceOption() { Action = () => LureMonster(team,amount), Description = "Ok" },
                    No
                },
                "Put " + amount.ToString("D") + " food out to attract monsters?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "You do not have any food to attract monsters.");
        }
    }
    
    private void LureMonster(PlayerTeam team, int food)
    {
        team.OnFoodFound.Invoke(-food);

        StartCoroutine(Spawning(team));
    }

    void StealTreasureBox()
    {
        if (Treasure <= 0)
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                "No treasure to steal.");
        }
        else
        {
            var options = new List<PlayerChoice.ChoiceOption>() {
                new PlayerChoice.ChoiceOption() { Action = StealTreasure, Description = "Shinys!!" },
                No
            };

            PlayerChoice.SetupPlayerChoice(options.ToArray(),
                $"Steal treasure from the {AreaName}?");
        }
    }

    internal void StealTreasure()
    {
        team.OnTreasureFound.Invoke(Treasure);
        Treasure = 0;

        SoundController.PlayStinger(SoundBank.Stinger.Sneaking);

        if (Random.value < 0.6f)
            StartCoroutine(Spawning(team,LivingDead));
    }


    void SacGoblinBox()
    {
        //TODO: maybe shuffle first
        var options = team.Members.OrderBy(g=>g.Xp).Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SacGoblin(g, this), Description = g.name + " the " + g.ClassType }).ToList();
        options.Add(No);

        PlayerChoice.SetupPlayerChoice(options.ToArray(),
            $"Sacrifice a Goblin to the {AreaName}?");
    }
    
    void SellGoblinBox()
    {
        //TODO: maybe shuffle first

        var options = team.Members.Where(g => g != team.Leader).Take(4).Select(g =>
            new PlayerChoice.ChoiceOption() { Action = () => PlayerTeam.Instance.SellGoblin(g, 2), Description = g.name + " the " + g.ClassType }).ToList();
        options.Add(No);

        PlayerChoice.SetupPlayerChoice(options.ToArray(),
            "Sell a Goblin for 2 goblin treasures?");
    }
    
    private void Buy(BuyableItem b)
    {
        //TODO: check that this works
        b.AmountForSale--;

        switch (b.Type)
        {
            case Tradable.Goblin:
                var gobbo = MapGenerator.GenerateCharacter(GoblinPrefab.gameObject, InArea, team.transform, true).GetComponent<Goblin>();
                gobbo.name = NameGenerator.GetName();
                gobbo.SelectClass(b.BuyableClass);
                team.AddMember(gobbo);
                break;
            case Tradable.Food:
                PlayerTeam.Instance.BuyFood(5, b.Price);
                break;
            case Tradable.Healing:
                PayToHeal(b.Price);
                break;
            case Tradable.Equipment:
                team.OnEquipmentFound.Invoke(EquipmentGen.GetEquipment(b.EquipmentType), team.Leader);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        team.OnTreasureFound.Invoke(-b.Price);
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
                $"Buy {b.Name} from {Inhabitants} for {b.Price} treasure?");
        }
        else
        {
            PlayerChoice.SetupPlayerChoice(new PlayerChoice.ChoiceOption[] { OkOption },
                $"You need {b.Price} treasure to buy {b.Name} from the {Inhabitants}.");
        }
    }


}
