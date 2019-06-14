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

    public Button NecromancyButton;
    public Button HideButton;


    public EnabledOnStates[] ControlledObjectMappings;
    
    // todo: change to on state change
    void FixedUpdate()
    {
        if( !PlayerController.Instance.Team.Leader)
            return;

        if (!PlayerController.Instance.Team.Leader.Alive() || PlayerController.Instance.Team.Challenger)
            foreach (var controlledObjectMapping in ControlledObjectMappings)
            {
                controlledObjectMapping.ControlledGameObject.SetActive(false);
            }
        else
            foreach (var enabledOnStates in ControlledObjectMappings)
            {
                enabledOnStates.ControlledGameObject.SetActive(enabledOnStates.States.Contains(PlayerController.Instance.Team.Leader.State));
            }

        if(PlayerController.Instance.Team.Leader.ClassType != Goblin.Class.Necromancer || !PlayerController.Instance.Team.Leader.InArea.AnyGoblins(true))
            NecromancyButton.gameObject.SetActive(false);

        if(!PlayerController.Instance.Team.Leader.InArea.RoadsTo.Any())
            HideButton.gameObject.SetActive(false);
    }
}
