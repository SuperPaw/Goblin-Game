using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoblinUIList : MonoBehaviour
{
    public PlayerTeam Team;
    public GoblinListEntry EntryObject;
    private List<GoblinListEntry> Entries = new List<GoblinListEntry>();
    public static GoblinUIList Instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (!Team)
            Team = FindObjectOfType<PlayerTeam>();

        if (!Instance)
            Instance = this;

        EntryObject.gameObject.SetActive(false);
    }

    public static void UpdateGoblinList()
    {
        Instance.UpdateList();
    }

    //TODO: use 
    private void UpdateList()
    {
        foreach (var e in Entries)
        {
            if(!e.Goblin)
                continue;

            if(e.Goblin.Alive() && e.Goblin.Team)
                UpdateEntry(e);
            else
                e.GetComponent<AiryUIAnimationManager>()?.SetActive(false);
        }

        if (!GameManager.Instance.GameStarted)
            return;

        if(!Entries.Any(ent => ent.Goblin.IsChief()))
            CreateGoblinEntry(Team.Leader);

        foreach (var m in Team.Members.Where(m=> !Entries.Any(ent => ent.Goblin == m)))
        {
            if (m == Team.Leader)
                continue;
            CreateGoblinEntry(m);
        }
    }

    private void UpdateEntry(GoblinListEntry e)
    {
        var g = e.Goblin;

        e.ClassImage.sprite = GameManager.GetClassImage(g.ClassType);

        //without "Chief "
        if ((g.IsChief()))
        {
            var strs = g.name.Split(' ').ToList();

            strs.Remove("Chief");

            var st = strs.First() + System.Environment.NewLine;
            strs.Remove(strs.First());

            foreach (var s in strs)
            {
                st += " " + s;
            }
            e.NameText.text = st;
        }
        else
        {
            e.NameText.text = g.ToString();
        }

        e.ChiefImage.gameObject.SetActive(g == Team.Leader);

        e.LevelUpReady.gameObject.SetActive(g.WaitingOnLevelup());// || g.WaitingOnClassSelection);
    }

    private void CreateGoblinEntry(Goblin g)
    {
        var e = Instantiate(EntryObject,EntryObject.transform.parent);
        
        e.Goblin = g;
        
        e.gameObject.SetActive(true);
        Entries.Add(e);
        
        g.OnDeath.AddListener(gob => e.MarkAsDead());

        UpdateEntry(e);
    }

    internal static void HighlightGoblin(Goblin goblin)
    {
        UIManager.HighlightText(Instance.Entries.First(g => g.Goblin == goblin).NameText);
    }
}
