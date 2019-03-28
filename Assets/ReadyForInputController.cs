using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReadyForInputController : MonoBehaviour
{
    [Serializable]
    public struct EnabledOnStates
    {
        public GameObject ControlledGameObject;
        public Character.CharacterState[] States;
    }

    public EnabledOnStates[] ControlledObjectMappings;
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if( !PlayerController.Instance.Team.Leader)
            return;

        if (!PlayerController.Instance.Team.Leader.Alive())
            foreach (var controlledObjectMapping in ControlledObjectMappings)
            {
                controlledObjectMapping.ControlledGameObject.SetActive(false);
            }


        foreach (var enabledOnStates in ControlledObjectMappings)
        {
            enabledOnStates.ControlledGameObject.SetActive(enabledOnStates.States.Contains(PlayerController.Instance.Team.Leader.State));
        }
    }
}
