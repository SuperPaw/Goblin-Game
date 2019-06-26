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
    private List<PointOfInterest.OptionType> OpenTypes = new List<PointOfInterest.OptionType>();
    public static UnityEvent CloseOptionsEvent;
    private List<PoiOptionButton> InstantiatedButtons = new List<PoiOptionButton>();

    void Awake()
    {
        if (CloseOptionsEvent == null)
        {
            CloseOptionsEvent = new UnityEvent();
            FindObjectOfType<PlayerTeam>()?.OnOrder.AddListener(CloseOptionsEvent.Invoke);
        }

        CloseOptionsEvent.AddListener(CloseOptions);

    }

    //TODO: create as coroutine for pop-ups
    public void CreateOption(PointOfInterest.OptionType type, UnityAction action)
    {
        if(OpenTypes.Contains(type)) return;

        OpenTypes.Add(type);

        var instance = Instantiate(OptionButton, this.transform);

        instance.gameObject.SetActive(true);

        instance.Button.onClick.AddListener(action);
        instance.Button.onClick.AddListener(CloseOptionsEvent.Invoke);
        instance.TargetImage.sprite = GameManager.Instance.OptionTargetImages.First(o => type ==o.type).image;

        InstantiatedButtons.Add(instance);

        OptionButton.gameObject.SetActive(false);
    }

    private void CloseOptions()
    {
        foreach (var b in InstantiatedButtons)
        {
            Destroy(b.gameObject);
        }
        InstantiatedButtons.Clear();
        OpenTypes.Clear();
    }
}
