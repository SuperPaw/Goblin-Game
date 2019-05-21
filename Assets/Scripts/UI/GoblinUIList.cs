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

    private void UpdateList()
    {

        foreach (var e in Entries)
        {
            Destroy(e.gameObject);
        }
        Entries.Clear();

        if (!GameManager.Instance.GameStarted)
            return;

        CreateGoblinEntry(Team.Leader);

        foreach (var m in Team.Members)
        {
            if (m == Team.Leader)
                continue;
            CreateGoblinEntry(m);
        }
    }

    private void CreateGoblinEntry(Goblin g)
    {
        var e = Instantiate(EntryObject,EntryObject.transform.parent);

        e.ClassImage.sprite = GameManager.GetClassImage(g.ClassType);

        //without "Chief "
        if ((g == Team.Leader))
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
            e.NameText.text = g.name;
        }
        
        
        e.Goblin = g;

        e.ChiefImage.gameObject.SetActive(g == Team.Leader);

        e.LevelUpReady.gameObject.SetActive(g.WaitingOnLevelup());// || g.WaitingOnClassSelection);

        g.OnDeath.AddListener(gob=>e.MarkAsDead());

        e.gameObject.SetActive(true);
        Entries.Add(e);
    }

}
