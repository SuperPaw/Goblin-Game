using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadUIScaler : MonoBehaviour {
    public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0f,0.6f,1f,1.2f);
    private Vector3 MaxScale;
    private float MinZoom, MaxZoom;
    private Camera Cam;

    void Start()
    {
        MaxScale = transform.localScale;
        var pControl = FindObjectOfType<PlayerController>();

        MinZoom = pControl.ZoomMinBound;
        MaxZoom = pControl.ZoomMaxBound;
        Cam = pControl.Cam;
    }

	// Update is called once per frame
	void Update ()
	{
	    transform.localScale = ScaleCurve.Evaluate((Cam.orthographicSize - MinZoom) / MaxZoom) * MaxScale;
	}
}
