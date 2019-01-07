
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

    internal Vector3 GetRandomPosInArea()
    {
        Vector3 v = transform.position + Random.insideUnitSphere * (Size/2);

        v.y = 0;

        return v;
    }
}
