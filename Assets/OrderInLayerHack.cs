using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderInLayerHack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var s = GetComponent<SpriteRenderer>();

        s.sortingOrder = -(int)transform.position.z;
    }
}
