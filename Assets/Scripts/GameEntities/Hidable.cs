using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hidable : MonoBehaviour
{

    public Transform HideLocation;
    //todo: should also change the chracter state, when this switches
    public Character OccupiedBy;

    public Area Area { get; internal set; }
}
