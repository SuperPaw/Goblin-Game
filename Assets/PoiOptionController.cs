using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PoiOptionController : MonoBehaviour
{
    public enum OptionType { Attack, Buy, Sell, Sacrifice,Lure,Explore}
    
    public enum OptionTarget { Goblin, Staff, Human,Food,Health,
        Monster,
        Cave
    }
    
    [Serializable]
    public struct TargetImaget
    {
        public OptionTarget type;
        public Sprite image;
    }
    [Serializable]
    public struct OptionImage
    {
        public OptionType type;
        public Sprite image;
    }

    public List<TargetImaget> OptionTargetImages;
    public List<OptionImage> OptionTypeImages;
    public PoiOptionButton OptionButton;
    public AnimationCurve PopupCurve;

    //TODO: create as coroutine for pop-ups
    public void CreateOption(OptionType type, OptionTarget target, UnityAction action)
    {
        var instance = Instantiate(OptionButton, this.transform);

        instance.gameObject.SetActive(true);

        instance.Button.onClick.AddListener(action);
        instance.TargetImage.sprite = OptionTargetImages.First(o => target ==o.type).image;
        instance.TypeImage.sprite = OptionTypeImages.First(o => type== o.type).image;

        OptionButton.gameObject.SetActive(false);
    }
}
