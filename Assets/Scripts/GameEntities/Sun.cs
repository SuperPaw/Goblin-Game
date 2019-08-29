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

    public static void TravelRoutine(Goblin leader)
    {
        Instance.StartCoroutine(Instance.TravelFade(leader));
    }

    private IEnumerator TravelFade(Goblin leader)
    {
        //Debug.Log("Travel distance: "+ distance + ", Fade time: "+ distance * TravelFadeFactor);

        var fadeInOutDuration = 2;

        var start = Time.unscaledTime;

        //Time speeds up
        while (Time.unscaledTime < start + fadeInOutDuration)
        {
            yield return new WaitForFixedUpdate();
            var x = TimeSpeedUpCurve.Evaluate((Time.unscaledTime - start) / fadeInOutDuration);

            Time.timeScale = x;
            //SoundController.Instance.MusicAudioSource.pitch = x;
        }

        yield return new WaitUntil(() => !leader.Travelling());

        start = Time.unscaledTime;

        //fading time back down
        while (Time.unscaledTime < start + fadeInOutDuration)
        {
            yield return new WaitForFixedUpdate();
            var x = TimeSpeedUpCurve.Evaluate((start - Time.unscaledTime) / fadeInOutDuration);

            Time.timeScale = x;
            //SoundController.Instance.MusicAudioSource.pitch = x;
        }
    }
}
