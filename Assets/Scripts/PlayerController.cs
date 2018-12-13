using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public TeamController Team;
    public Camera Cam;

    [Header("Controls")]
    public int MouseMoveKey = 1;
    public enum MappableActions { Hide, Attack, Flee, Menu }
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

    void Start()
    {
        Team = GetComponent<TeamController>();

        if(!Team) Debug.LogWarning("Unable to find team for player controls!");

        if (!Cam) Cam = Camera.main;

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
                if (hit.collider && hit.collider.GetComponent<Character>())
                {
                    var c = hit.collider.GetComponent<Character>();
                    if (c)
                    {
                        Debug.Log("Clicked charaacter " + c.name);

                        if (c.tag != "Player")
                            foreach (var gobbo in Team.Members)
                            {
                                gobbo.State = Character.CharacterState.Attacking;
                                gobbo.AttackTarget = c;
                            }
                    }
                }
                else
                {
                    var target = hit.point;
                    target.y = 0;

                    Team.Move(target);
                }
            }
        }


        foreach (var mapping in KeyMappings)
        {
            if (Input.GetKeyDown(mapping.Key))
            {
                switch (mapping.Action)
                {
                    case MappableActions.Hide:
                        Team.Hide();
                        break;
                    case MappableActions.Attack:
                        break;
                    case MappableActions.Flee:
                        break;
                    case MappableActions.Menu:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void Zoom(float deltaMagnitudeDiff, float speed)
    {
        if(Math.Abs(deltaMagnitudeDiff) < 0.001) return;

        Cam.orthographicSize += deltaMagnitudeDiff * speed;
        // set min and max value of Clamp function upon your requirement
        //Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize, ZoomMinBound, ZoomMaxBound);
    }
}
