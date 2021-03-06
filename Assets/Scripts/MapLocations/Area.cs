﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Area : MonoBehaviour
{
    public static readonly int MaxNeighbours = 6;

    public int X, Y;

    // calculated values while finding path
    [HideInInspector]
    public int gCost;
    [HideInInspector]
    public int hCost;
    [HideInInspector]
    public Area parent;
    [HideInInspector]
    public int fCost
    {
        get { return gCost + hCost; }
    }

    //public bool ContainsRoad;

    public List<Character> PresentCharacters;
    public List<Lootable> Lootables;
    public List<Hidable> Hidables;
    public List<MapGenerator.Tile> MovablePositions = new List<MapGenerator.Tile>();
    public HashSet<Area> Neighbours = new HashSet<Area>();
    public HashSet<Area> RoadsTo = new HashSet<Area>();
    public BoxCollider Collider;

    public Sprite BasicAreaSprite, RoadAreaSprite;

    public PointOfInterest PointOfInterest;
    public PlayerController Controller;

    public SpriteRenderer FogOfWarSprite;
    public Color LightFogColor, UnseenFogColor;
    //should be the size of x and z of the box collider
    public int Size;

    public GameObject AreaUIObject;
    public Image AreaIcon;
    public TextMeshProUGUI AreaText;

    public void SetUpUI()
    {
        if (!Controller)
            Controller = FindObjectOfType<PlayerController>();

        if (PointOfInterest)
        {
            AreaIcon.sprite = PointOfInterest.IconSprite;
            AreaText.text = PointOfInterest.AreaName;
        }
        else if(RoadsTo.Any())
        {
            AreaIcon.sprite = RoadAreaSprite;
            AreaText.text = "";
        }
        else
        {
            AreaIcon.sprite = BasicAreaSprite;
            AreaText.text = "";
        }
    }

    internal Vector3 GetRandomPosInArea()
    {
        var pos = MovablePositions[Random.Range(0, MovablePositions.Count)];
        

        return new Vector3(pos.X,0,pos.Y);
    }


    public void RemoveFogOfWar(bool inArea)
    {
        FogOfWarSprite.gameObject.SetActive(!inArea);

        FogOfWarSprite.color = LightFogColor;
        
        PresentCharacters.ForEach(c => c.HolderGameObject.SetActive(true));
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

        //if character is not supposed to go there. This hack Creates problems 
        //if(c.Idling())
        //    return;

        c.InArea?.PresentCharacters.Remove(c);

        //should be the only place we set InArea
        c.InArea = this;
        if(!PresentCharacters.Contains(c))
            PresentCharacters.Add(c);

        c.IrritationMeter = 0;
        if(c.Fleeing() || c.Travelling())
            c.ChangeState(Character.CharacterState.Idling);

        c.HolderGameObject.SetActive(c.Team || Visible());

        //TODO: check if leader
        PlayerController.UpdateFog();


        //TODO: move to area change method
        if (PointOfInterest)
            (c as Goblin)?.Speak(PlayerController.GetLocationReaction(this.PointOfInterest.PoiType));
        else if (AnyEnemies()) //TODO:select random enemy
            (c as Goblin)?.Speak(PlayerController.GetEnemyReaction(PresentCharacters.First(ch => ch.tag == "Enemy" && ch.Alive()).CharacterRace));

    }

    //public void OnTriggerExit(Collider other)
    //{
    //    if (!other.gameObject.GetComponent<Character>())
    //    {
    //        Debug.LogWarning(name + ": Object with no character collided; " + other.gameObject);
    //    }

    //    Character c = other.gameObject.GetComponent<Character>();

    //    //Debug.Log(c + " left " + name);

    //    PresentCharacters.Remove(c);
    //}

    public void MoveTo()
    {
        Controller.ClickedArea(this);
    }

    public bool AnyEnemies(bool onlyConsiderWandering = false)
    {
        if(onlyConsiderWandering)
            return PresentCharacters.Any(c => c.tag == "Enemy" && c.Alive() && c.StickToRoad);

        return PresentCharacters.Any(c => c.tag == "Enemy" && c.Alive());
    }

    internal Area GetClosestNeighbour(Vector3 position, bool useRoads = false)
    {
        Area closest = null;
        float distance = Mathf.Infinity;

        var ns = useRoads && RoadsTo.Any() ? RoadsTo : Neighbours;

        foreach (var go in ns)
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

    public bool HasMaximumConnections => Neighbours.Count >= MaxNeighbours;

    public bool ContainsRoads => RoadsTo.Any();

    public void EnableAreaUI()
    {
        AreaUIObject.SetActive(true);
    }

    //TODO: should this just be handled by events
    public void DisableAreaUI()
    {
        AreaUIObject.SetActive(false);
    }
}
