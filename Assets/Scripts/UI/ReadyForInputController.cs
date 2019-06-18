using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReadyForInputController : MonoBehaviour
{
    [Serializable]
    public struct EnabledOnStates
    {
        public AiryUIAnimationManager ControlledGameObject;
        public Character.CharacterState[] States;
    }

    public AiryUIAnimationManager NecromancyButton;
    public AiryUIAnimationManager HideButton;


    public EnabledOnStates[] ControlledObjectMappings;
    
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
            foreach (var controlledObjectMapping in ControlledObjectMappings)
            {
                controlledObjectMapping.ControlledGameObject.SetActive(false);
            }
        else
            foreach (var enabledOnStates in ControlledObjectMappings)
            {
                if (enabledOnStates.ControlledGameObject == NecromancyButton)
                    NecromancyButton.SetActive(enabledOnStates.States.Contains(PlayerController.Instance.Team.Leader.State)
                        && PlayerController.Instance.Team.Leader.ClassType == Goblin.Class.Necromancer && PlayerController.Instance.Team.Leader.InArea.AnyGoblins(true));
                else if (enabledOnStates.ControlledGameObject == HideButton )
                    HideButton.SetActive(enabledOnStates.States.Contains(PlayerController.Instance.Team.Leader.State)
                        && PlayerController.Instance.Team.Leader.InArea.RoadsTo.Any()
                        &! PlayerController.Instance.Team.Leader.InArea.AnyEnemies());
                else
                    enabledOnStates.ControlledGameObject.SetActive(enabledOnStates.States.Contains(PlayerController.Instance.Team.Leader.State));
            }
    }
}
