using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public static Sun Instance;
    public Light Light;
    public Color DayColor, NightColor;

    private void Start()
    {
        Instance = this;
    }

    public static void Night()
    {
        SoundController.ChangeBackground(SoundBank.Background.Night);

        Instance.StartCoroutine(Instance.ChangeLightToColor(Instance.NightColor));
    }

    public IEnumerator ChangeLightToColor(Color endColor)
    {
        var duration = 6;
        var StartColor = Light.color;
        var start = Time.time;

        while (Time.time < start +duration)
        {
            //Debug.Log(Color.Lerp(StartColor, endColor, (Time.time - start) / duration));
            yield return new WaitForFixedUpdate();
            Light.color = Color.Lerp(StartColor,endColor,(Time.time-start)/duration);
        }
    }

    public static void Day()
    {
        SoundController.ChangeBackground(SoundBank.Background.Forest);
        Instance.StartCoroutine(Instance.ChangeLightToColor(Instance.DayColor));
    }
}
