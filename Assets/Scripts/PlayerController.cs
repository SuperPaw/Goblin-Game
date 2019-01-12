﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public TeamController Team;
    public Camera Cam;

    [Header("Controls")]
    public int MouseMoveKey = 1;
    public enum MappableActions { Hide, Attack, Flee, Menu,FixCamOnLeader }
    [Serializable]
    public struct KeyMapping
    {
        public KeyCode Key;
        public MappableActions Action;
    }
    public KeyMapping[] KeyMappings;

    private bool _mouseHeld;
    private Vector3 _mouseDragPos;

    public float ZoomMinBound= 2;
    public float ZoomMaxBound = 50;
    public float MouseZoomSpeed = 1;
    public bool FollowLeader;

    [Header("Follow Animation")] public float MoveTime = 1;
    public AnimationCurve MoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);



    public void Initialize()
    {
        if (!Instance)
            Instance = this;

        Team = GetComponent<TeamController>();

        if(!Team) Debug.LogWarning("Unable to find team for player controls!");

        if (!Cam) Cam = Camera.main;
        
        //UpdateFogOfWar();
        MoveToLeader();
    }

    void FixedUpdate()
    {
        //maybe at larger interval
        if(FollowLeader)
            MoveToLeader();
    }


    void Update()
    {
        //TODO: divide these into methods
        if (Input.touchSupported)
        { }

        
        Zoom(Input.mouseScrollDelta.y, MouseZoomSpeed);
        
        if (Input.GetMouseButtonDown(0))
        {
            _mouseHeld = true;
            _mouseDragPos = Cam.ScreenToWorldPoint(Input.mousePosition);
            FollowLeader = false;
        }
        if (Input.GetMouseButtonUp(0)) _mouseHeld = false;

        if (_mouseHeld)
        {
            var moveDelta = (_mouseDragPos - Cam.ScreenToWorldPoint(Input.mousePosition));
            moveDelta.y = 0;

            //TODO: the z axis does not move correctly due to the rotation of the camera

            Cam.transform.position += moveDelta;

        }


        if (Input.GetMouseButtonDown(1))
        {
            //TODO Move this to a routine and insert a pause here and a shout from the Chief

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                if (hit.collider && hit.collider.GetComponent<Character>()) //TODO:check visibility else just move there
                {


                        var c = hit.collider.GetComponent<Character>();
                    if (c)
                    {
                        Debug.Log("Clicked charaacter " + c.name);

                        if (c.tag != "Player")
                        {
                            if(c.InArea == Team.Leader.InArea)
                                Team.Attack(c);
                            else
                            {
                                Debug.LogWarning("Trying to attack character in another area");
                            }
                        }
                        else 
                        {
                            CharacterView.ShowCharacter(c as Goblin);
                        }
                    }
                }
                else if (hit.collider && hit.collider.GetComponent<Area>())
                {
                    var a = hit.collider.GetComponent<Area>();
                    //Debug.Log("Clicked Area : " + (a.name));

                    var target = a.transform.position;//hit.point;
                    target.y = 0;

                    if (!Team.Leader.InArea.ConnectsTo.Contains(a))
                    {
                        Debug.LogWarning("Not possible to move to "+ a + " from "+ Team.Leader.InArea);
                        return;
                    }

                    Team.Move(target,a);
                }
                else
                {
                    Debug.LogWarning("Not a valid hit: "+hit.point);
                }

            }
        }


        foreach (var mapping in KeyMappings)
        {
            if (Input.GetKeyDown(mapping.Key) )
            {
                Action(mapping.Action);
            }
        }
    }

    public static void UpdateFog()
    {
        Instance.UpdateFogOfWar();
    }

    private void UpdateFogOfWar()
    {
        if (!Team.Leader.InArea)
        {
            Debug.LogWarning("Chief not present in any area");
            return;
        }

        Team.Leader.InArea.RemoveFogOfWar(true);

        foreach (var connecting in Team.Leader.InArea.ConnectsTo){     
            connecting.RemoveFogOfWar(false);
        }
    }

    public void Action(MappableActions action)
    {
        switch (action)
        {
            case MappableActions.Hide:
                Debug.Log("Hiding");
                Team.Hide();
                break;
            case MappableActions.Attack:
                Team.Attack();
                break;
            case MappableActions.Flee:
                Team.Flee();
                break;
            case MappableActions.Menu:
                break;
            case MappableActions.FixCamOnLeader:
                FollowLeader = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //TODO: this should be less hacky
    public void Action(string action)
    {
        Action((MappableActions)Enum.Parse(typeof(MappableActions),action,true) );
    }

    //TODO: move top camera controller
    public void Zoom(float deltaMagnitudeDiff, float speed)
    {
        if(Math.Abs(deltaMagnitudeDiff) < 0.001) return;

        Cam.orthographicSize += deltaMagnitudeDiff * speed;
        // set min and max value of Clamp function upon your requirement
        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize, ZoomMinBound, ZoomMaxBound);
    }


    private void MoveToLeader()
    {
        StartCoroutine(MoveCamera(Team.Leader.transform.position));
    }

    private IEnumerator MoveCamera(Vector3 loc)
    {
        //currentLocation = loc;
        var offset = 50;

        var start = Cam.transform.position;
        var end = new Vector3(loc.x, start.y,loc.z-offset);
        for (var t = 0f; t < MoveTime; t += Time.deltaTime)
        {
            yield return null;
            Cam.transform.position = Vector3.LerpUnclamped(start, end, MoveCurve.Evaluate(t / MoveTime));
        }

        Cam.transform.position = end;
    }
    
}
