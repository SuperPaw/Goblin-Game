
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI Loading;
    public TextMeshProUGUI LoadingDescription;
    public MapGenerator MapGen;
    
	void Start ()
	{
	    //yield return new WaitForFixedUpdate();
        
		StartGame();
        
	}

    void StartGame()
    {
        //could load different scenes instead of just running the generate

        SoundController.ChangeMusic(SoundBank.Music.Menu);

        StartCoroutine(MapGen.GenerateMap(SetLoadingText, ()=>gameObject.SetActive(false)));
        //TODO: include gobbo creation in loading

    }

    private void SetLoadingText(int pct, string descrip)
    {
        Loading.text = pct + "%";
        LoadingDescription.text = descrip;
    }
}
