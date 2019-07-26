using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MonsterGen : MonoBehaviour
{
    public Dropdown Dropd;
    public Button GenerateButton;
    public Character[] Monsters;
    private Character selected;

    public void Start()
    {
        Dropd.AddOptions(Monsters.Select(mon=> new Dropdown.OptionData(mon.name)).ToList());

        Dropd.onValueChanged.AddListener(i => selected = Monsters[i]);

        StartCoroutine(FindObjectOfType<MapGenerator>().GenerateMap((i,s)=> Debug.Log(i + s), () => Debug.Log("done")));
    }

    public void GenerateSelectedMonster()
    {
        Debug.Log($"Generating {selected}");
    }
}

