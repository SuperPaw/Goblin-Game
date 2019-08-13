using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ActionStateProcessor : MonoBehaviour
{
    private static Dictionary<Character.CharacterState,Func<Character,Coroutine>> actionStates = new Dictionary<Character.CharacterState, Func<Character,Coroutine>>();
    private static bool initialized;
    private static ActionStateProcessor Instance;

    private void Start()
    {
        if (!Instance) Instance = this;
    }

    private static void Initialize()
    {
        actionStates.Clear();

        var assembly = Assembly.GetAssembly(typeof(ActionState));

        var allStates = assembly.GetTypes()
            .Where(t => typeof(ActionState).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var state in allStates)
        {
            var stateInstance =Activator.CreateInstance(state) as ActionState;

            if(actionStates.ContainsKey(stateInstance.StateType)) Debug.LogError($"Multiple instances of state action: {stateInstance.StateType}");

            actionStates.Add(stateInstance.StateType,c=>Instance.StartCoroutine(stateInstance.StateRoutine(c)));
        }
    }

    public static Coroutine CreateStateRoutine(Character ch, Character.CharacterState state)
    {
        if(!initialized) Initialize();

        if (!actionStates.ContainsKey(state))
        {
            Debug.Log($"No state routine avaiable for : {state}, {ch}");
            return null;
        }

        return actionStates[state].Invoke(ch);
    }
}
