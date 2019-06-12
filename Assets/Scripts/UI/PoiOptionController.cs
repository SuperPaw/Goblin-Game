using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PoiOptionController : MonoBehaviour
{
    public PoiOptionButton OptionButton;
    public AnimationCurve PopupCurve;
    private List<GameManager.OptionType> OpenTypes = new List<GameManager.OptionType>();

    //TODO: create as coroutine for pop-ups
    public void CreateOption(GameManager.OptionType type, UnityAction action)
    {
        if(OpenTypes.Contains(type)) return;

        OpenTypes.Add(type);

        var instance = Instantiate(OptionButton, this.transform);

        instance.gameObject.SetActive(true);

        instance.Button.onClick.AddListener(action);
        instance.TargetImage.sprite = GameManager.Instance.OptionTargetImages.First(o => type ==o.type).image;

        OptionButton.gameObject.SetActive(false);
    }
}
