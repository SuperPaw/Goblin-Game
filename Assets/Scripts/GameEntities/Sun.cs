using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public static Sun Instance;
    public Light Light;
    public Color DayColor, NightColor;
    [Header("Travel parameters")]
    public float TravelFadeFactor = 0.5f;
    public AnimationCurve TimeSpeedUpCurve;

    private void Awake()
    {
        Instance = this;
    }

    public static void Night(float duration = 6)
    {
        SoundController.ChangeBackground(SoundBank.Background.Night);
        SoundController.ChangeMusic(SoundBank.Music.NoMusic);

        Instance.StartCoroutine(Instance.ChangeLightToColor(Instance.NightColor,duration));
    }

    public IEnumerator ChangeLightToColor(Color endColor, float duration)
    {
        var StartColor = Light.color;
        var start = Time.time;

        while (Time.time < start +duration)
        {
            //Debug.Log(Color.Lerp(StartColor, endColor, (Time.time - start) / duration));
            yield return new WaitForFixedUpdate();
            Light.color = Color.Lerp(StartColor,endColor,(Time.time-start)/duration);
        }
    }

    public static void Day(float duration = 6)
    {
        SoundController.ChangeBackground(SoundBank.Background.Forest);
        SoundController.ChangeMusic(SoundBank.Music.Explore);
        Instance.StartCoroutine(Instance.ChangeLightToColor(Instance.DayColor,duration));
    }

    public static void TravelRoutine(float travelDist)
    {
        Instance.StartCoroutine(Instance.TravelFade(travelDist));
    }

    private IEnumerator TravelFade(float distance)
    {
        //Debug.Log("Travel distance: "+ distance + ", Fade time: "+ distance * TravelFadeFactor);

        var duration = distance * TravelFadeFactor;

        //Night(duration);
        var start = Time.time;

        //TODO: should it be a curve down or simply stop once the chief stops travelling
        while (Time.time < start + duration)
        {
            yield return new WaitForFixedUpdate();
            var x = TimeSpeedUpCurve.Evaluate((Time.time - start) / duration);

            Time.timeScale = x;
            //SoundController.Instance.MusicAudioSource.pitch = x;
        }

        //Day();
    }
}
