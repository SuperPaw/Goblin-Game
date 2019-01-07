using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    public int X;
    public int Y;
    public List<Character> PresentCharacters;
    public List<Area> ConnectsTo;
    public BoxCollider Collider;
    //should be the size of x and z of the box collider
    public int Size;

}
