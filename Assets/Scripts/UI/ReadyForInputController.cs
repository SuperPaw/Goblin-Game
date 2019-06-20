using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReadyForInputController : MonoBehaviour
{
    public ActionButton[] ActionButtons;
    
    // todo: change to on state change
    void FixedUpdate()
    {
        if (!GameManager.Instance.GameStarted)
            return;

        if (!PlayerController.Instance.Team.Leader)
        {
            Debug.Log("No leader in tribe");
            return;
        }

        if (!PlayerController.Instance.Team.Leader.Alive() || PlayerController.Instance.Team.Challenger)
            foreach (var controlledObjectMapping in ActionButtons)
            {
                controlledObjectMapping.AiryUIAnimationManager.SetActive(false);
            }
        else
            foreach (var actionButton in ActionButtons)
            {
                actionButton.AiryUIAnimationManager.SetActive(PlayerController.ActionIsLegal(actionButton.Action));
            }
    }
}
