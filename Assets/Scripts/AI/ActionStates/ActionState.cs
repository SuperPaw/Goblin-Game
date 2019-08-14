using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionState 
{
    public ActionState() { }

    //public ActionState(Character ch)
    //{
    //    Character = ch;
    //}

    //public Character Character;

    public abstract Character.CharacterState StateType { get; }

    public abstract IEnumerator StateRoutine(Character g);

    //public abstract void EndState();
}
