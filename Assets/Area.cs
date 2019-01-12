
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Area : MonoBehaviour
{
    public int X;
    public int Z;
    public List<Character> PresentCharacters;
    public List<Lootable> Lootables;
    public List<Hidable> Hidables;
    public List<Area> ConnectsTo;
    public BoxCollider Collider;

    public SpriteRenderer FogOfWarSprite;
    public Color LightFogColor, UnseenFogColor;
    //should be the size of x and z of the box collider
    public int Size;

    internal Vector3 GetRandomPosInArea()
    {
        Vector3 v = transform.position + Random.insideUnitSphere * (Size/2);
        
        v.y = 0;
        
        NavMeshHit myNavHit;
        if (NavMesh.SamplePosition(v, out myNavHit, 3, -1))
        {
            v = myNavHit.position;
        }
        else
            Debug.Log("Not able to go to position: " + v);


        return v;
    }


    public void RemoveFogOfWar(bool InArea)
    {
        FogOfWarSprite.gameObject.SetActive(!InArea);

        FogOfWarSprite.color = LightFogColor;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetComponent<Character>())
        {
            Debug.LogWarning(name + ": Object with no character collided; "+ other.gameObject);
        }

        Character c = other.gameObject.GetComponent<Character>();

        //should be the only place we set InArea
        c.InArea = this;
        PresentCharacters.Add(c);

        //TODO: check if leader
        PlayerController.UpdateFog();

        //Debug.Log(c +" entered "+ name);
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.GetComponent<Character>())
        {
            Debug.LogWarning(name + ": Object with no character collided; " + other.gameObject);
        }

        Character c = other.gameObject.GetComponent<Character>();

        //Debug.Log(c + " left " + name);

        PresentCharacters.Remove(c);
    }

    public bool AnyEnemies()
    {
        return PresentCharacters.Any(c => c.tag != "Player");
    }
}
