
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Text Loading;
    public MapGenerator MapGen;

	// Use this for initialization
	void Start () {
		StartGame();
        
	}

    void StartGame()
    {
        //could load different scenes instead of just running the generate

        StartCoroutine(MapGen.GenerateMap((int i) => Loading.text = i+ "%", ()=>gameObject.SetActive(false)));
        //TODO: include gobbo creation in loading
    }
}
