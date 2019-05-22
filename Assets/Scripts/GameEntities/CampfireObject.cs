using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class CampfireObject : MonoBehaviour
{
    [HideInInspector]
    public PlayerTeam Team;
    public Light Light;
    public SpriteRenderer FireSprite;

    internal void SetupCamp(int v)
    {
        StartCoroutine(CampRoutine(v));
    }

    private IEnumerator CampRoutine(int seconds)
    {
        var MaxEatPrGob = 4;


        Team.OnFoodFound.Invoke(-Mathf.Min(Team.Food, Team.Members.Count * MaxEatPrGob));

        Sun.Night();

        //FADE up fire
        //TODO: move fade to method
        var start = Time.time;
        var fadeDuration = 3;
        var fireIntensity = Light.intensity;

        while (Time.time < start + fadeDuration)
        {
            yield return new WaitForFixedUpdate();
            Light.intensity = fireIntensity * ((Time.time - start) / fadeDuration);
        }

        //Consume resources - add bonuses
        yield return new WaitForSeconds(seconds-fadeDuration);

        //TODO: light flicker
        //TODO: campfire sounds

        foreach (var teamMember in Team.Members)
        {
            teamMember.Heal();
        }

        Team.Campfire = null;

        Sun.Day();

        FireSprite.enabled = false;

        //Fading light
        start = Time.time;

        while (Time.time < start + fadeDuration)
        {
            yield return new WaitForFixedUpdate();
            Light.intensity = fireIntensity * (1 - (Time.time - start) / fadeDuration);
        }
        Light.enabled = false;


        //Destroy(this.gameObject);
    }
}
