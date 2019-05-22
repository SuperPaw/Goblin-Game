using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableWhenPaused : MonoBehaviour
{
    public GameObject Holder;

    // Update is called once per frame
    void Update()
    {
        Holder.SetActive(GameManager.Instance.GamePaused);
    }
}
