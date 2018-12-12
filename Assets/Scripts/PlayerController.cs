using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public TeamController Team;
    public Camera Cam;
    public int MouseMoveKey = 1;
    //TODO: should be related to distance of travel
    public float RandomMoveFactor = 2f;

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
        if(Input.touchSupported)
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
            Cam.transform.position += (_mouseDragPos - Cam.ScreenToWorldPoint(Input.mousePosition));
        }


        if (Input.GetMouseButtonDown(1))
        {
            //TODO Move this to a routine and insert a pause here and a shout from the Chief

            RaycastHit2D hit = Physics2D.Raycast(Cam.transform.position, Cam.ScreenToWorldPoint(Input.mousePosition));

            if (hit && hit.collider && hit.collider.GetComponent<Character>())
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
            else //MOVE
            {
                var target = Cam.ScreenToWorldPoint(Input.mousePosition);


                var leaderPos = Team.Leader.transform.position;

                //TODO: check for distance so no move right next to group

                foreach (var gobbo in Team.Members)
                {
                    gobbo.State = Character.CharacterState.Travelling;

                    //TODO: should use a max distance from leader to include group them together if seperated
                    //TODO: could just use a local instead of gloabl pos for the entire team and move that
                    gobbo.Target =
                        target + (gobbo.transform.position - leaderPos) * (Random.Range(0, RandomMoveFactor));
                }
            }
        }
        
    }


    private void Zoom(float deltaMagnitudeDiff, float speed)
    {
        if(deltaMagnitudeDiff == 0f) return;

        Cam.orthographicSize += deltaMagnitudeDiff * speed;
        // set min and max value of Clamp function upon your requirement
        //Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize, ZoomMinBound, ZoomMaxBound);
    }
}
