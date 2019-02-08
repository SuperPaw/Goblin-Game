using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeAnimation : MonoBehaviour
{
    public AnimationCurve Animation = AnimationCurve.EaseInOut(0,0.8f,1,1.2f);
    public float SizeAnimationTime = 1;
    public RectTransform RectTransform;
    public Vector2 Startsize;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform = GetComponent<RectTransform>();
        Startsize = RectTransform.sizeDelta;
    }

    // Update is called once per frame
    void Update()
    {
        RectTransform.sizeDelta = Startsize * Animation.Evaluate(Time.time % SizeAnimationTime);

    }
}
