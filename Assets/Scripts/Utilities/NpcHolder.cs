using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcHolder : MonoBehaviour
{
    public static NpcHolder Instance;

    private void Awake()
    {
        if(!Instance) Instance = this;
    }
}
