using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    public PlayerController.MappableActions Action;
    public Button Button;
    public AiryUIAnimationManager AiryUIAnimationManager;

    void Awake()
    {
        Button.onClick.AddListener(()=> PlayerController.Instance.Action(Action));
    }
}
