using System.Collections;
using UnityEngine;
using static Character;
using Random = UnityEngine.Random;



public class StateController
{
    private readonly Character _owner;
    private Coroutine stateChangeRoutine;
    public CharacterState State { get; private set; } = CharacterState.StartState;

    public StateController(Character owner)
    {
        _owner = owner;
    }


    /// <summary>
    /// to be used for orders
    /// will do nothing if state == dead
    /// </summary>
    /// <param name="newState">The new character state</param>
    public void ChangeState(CharacterState newState, bool immedeately = false, float time = 0f)//, int leaderAncinitet = 10)
    {
        if (!_owner.Alive())
        {
            return;
        }

        //check if state is already being changed
        //if(stateChangeRoutine != null)
        //    Debug.Log(name + ": Changing State already: newState "+newState + ", old: "+ State);

        stateChangeRoutine = _owner.StartCoroutine(
            immedeately ?
            StateChangingRoutine(newState, 0)
            : time > 0f ?
            StateChangingRoutine(newState, time)
            : StateChangingRoutine(newState, Random.Range(1.5f, 4f)));
    }


    private IEnumerator StateChangingRoutine(CharacterState newState, float wait)
    {
        var fromState = State;

        yield return new WaitForSeconds(wait);

        if (fromState != State)
        {
            Debug.Log(_owner + " no longer " + fromState + "; Now: " + State + "; Not doing" + newState);
            yield break;
        }

        if(newState == CharacterState.StartState)
        {
            Debug.LogError($"{_owner}: changeing state to start state");
        }

        if (State == newState)
        {
            Debug.Log(_owner + " is already " + newState);
            yield break;
        }

        if (_owner.Morale <= 0 && newState != CharacterState.Fleeing)
        {
            //Debug.Log(name + " not able to change state to " + newState + " Fleeing!");
            yield break;
        }

        //death is unescapable!!
        if (State != CharacterState.Dead)
        {
            State = newState;
        }

        _owner.navMeshAgent.isStopped = false;


        if (newState != CharacterState.Travelling && newState != CharacterState.Attacking &&
            newState != CharacterState.Fleeing)
        {
            _owner.TravellingToArea = null;
        }

        //Debug.Log($"Starting state: {newState}");
        //TODO: Assign to field and close the last state

        //if(StateRoutine != null) ActionStateProcessor.Instance.StopCoroutine(StateRoutine);

        //TODO: just assign and set to null when applicable
        var s = ActionStateProcessor.CreateStateRoutine(_owner, newState);
        if (s != null)
        {
            _owner.StateRoutine = s;
        }

        if (_owner as Goblin && PlayerController.IsStateChangeShout(State))
        {
            (_owner as Goblin)?.Speak(PlayerController.GetStateChangeReaction(State));
        }
        else if (State == CharacterState.Attacking)
        {
            _owner.Aggressive = true;
            yield return new WaitForSeconds(Random.Range(0f, 1.5f));
            if (State == CharacterState.Attacking)
            {
                (_owner as Goblin)?.Speak(SoundBank.GoblinSound.Roar);
            }
        }

        //TODO: should have an event instead for when statechanging is done;
        _owner.SetAnimationRandom();

        stateChangeRoutine = null;
    }

}

