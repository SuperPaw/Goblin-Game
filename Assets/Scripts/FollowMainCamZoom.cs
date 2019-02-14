using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMainCamZoom : MonoBehaviour
{
    public Camera Main;
    private Camera _thisCam;


    void Start()
    {
        _thisCam = GetComponent<Camera>();

        if(!Main)
            Main = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        _thisCam.orthographicSize = Main.orthographicSize;
    }
}
