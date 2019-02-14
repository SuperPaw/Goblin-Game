using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Area : MonoBehaviour
{
    //public int X;
    //public int Z;
    public List<Character> PresentCharacters;
    public List<Lootable> Lootables;
    public List<Hidable> Hidables;
    public List<MapGenerator.Tile> MovablePositions = new List<MapGenerator.Tile>();
    public HashSet<Area> Neighbours = new HashSet<Area>();
    public BoxCollider Collider;
    public PointOfInterest PointOfInterest;

    public SpriteRenderer FogOfWarSprite;
    public Color LightFogColor, UnseenFogColor;
    //should be the size of x and z of the box collider
    public int Size;

    internal Vector3 GetRandomPosInArea()
    {
        var pos = MovablePositions[Random.Range(0, MovablePositions.Count)];
        

        return new Vector3(pos.X,0,pos.Y);
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

        if (!c)
        {
            Debug.LogWarning(other.gameObject +" does not have a character object.");
            return;
        }

        //should be the only place we set InArea
        c.InArea = this;
        PresentCharacters.Add(c);

        c.IrritationMeter = 0;

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

    public bool AnyEnemies(bool onlyConsiderWandering = false)
    {
        if(onlyConsiderWandering)
            return PresentCharacters.Any(c => c.tag == "Enemy" && c.Alive() && c.Wandering);

        return PresentCharacters.Any(c => c.tag == "Enemy" && c.Alive());
    }

    internal Area GetClosestNeighbour(Vector3 position)
    {
        Area closest = null;
        float distance = Mathf.Infinity;
        foreach (var go in Neighbours)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return closest;
    }

    internal bool Visible()
    {
        return !FogOfWarSprite.gameObject.activeInHierarchy;
    }

    public bool PointIsInArea(Vector3 point)
    {
        var pos = transform.position;
        var adj = Size / 2f;
        
        return point.x > pos.x - adj && point.x < pos.x + adj
            && point.z > pos.z - adj && point.z < pos.z + adj;
    }
}
