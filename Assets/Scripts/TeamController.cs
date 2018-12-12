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
    public GameObject DefaultCharacter;
    [Range(0,20)]
    public int CharacterToGenerate;

    //TODO: should be related to distance of travel
    public float RandomMoveFactor = 2f;

    private Vector3 targetPos;

    // Use this for initialization
    void Start ()
    {
        for (int i = 0; i < CharacterToGenerate; i++)
        {
            var next = Instantiate(DefaultCharacter, transform);

            next.transform.position = new Vector3(i, 0, i%3);

            Members.Add(next.GetComponent<Character>());
        }

        if(CharacterToGenerate == 0) Members = GetComponentsInChildren<Character>().ToList();

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


        Debug.DrawLine(targetPos, targetPos + Vector3.up);

    }

    private void MemberDied(Character c)
    {
        Members.Remove(c);

        Debug.Log("Team member : "+ c.name + " died");

        Members.ForEach(m=> m.Moral -= m.MoralLossOnFriendDeath);
    }

    public void Move(Vector3 target)
    {
        var leaderPos = Leader.transform.position;

        targetPos = target;

        //TODO: check for distance so no move right next to group

        foreach (var gobbo in Members)
        {
            gobbo.State = Character.CharacterState.Travelling;

            //TODO: should use a max distance from leader to include group them together if seperated
            //TODO: could just use a local instead of gloabl pos for the entire team and move that
            gobbo.Target =
                target + (gobbo.transform.position - leaderPos) * (Random.Range(0, RandomMoveFactor));
        }
    }
}
