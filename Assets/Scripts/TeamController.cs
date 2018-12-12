using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TeamController : MonoBehaviour
{
    public List<Character> Members;
    public Character Leader;
    private bool updatedListeners;


    // Use this for initialization
    void Start ()
    {
        Members = GetComponentsInChildren<Character>().ToList();

        if (!Leader)
            Leader = Members.First();

    }

    private void Update()
    {
        if(!updatedListeners)
        {
            foreach (var character in Members)
            {
                character.Team = this;

                if (character.OnDeath == null) continue;
                character.OnDeath.AddListener(MemberDied);
            }
            updatedListeners = true;
        }
    }
	
    private void MemberDied(Character c)
    {
        Members.Remove(c);

        Debug.Log("Team member : "+ c.name + " died");

        Members.ForEach(m=> m.Moral -= m.MoralLossOnFriendDeath);
    }
}
