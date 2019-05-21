
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI Loading;
    public TextMeshProUGUI LoadingDescription;
    public MapGenerator MapGen;
    public RectTransform[] ZoomRects;
    private int imageStartSize;
    public int imageEndSize;
    [SerializeField]
    private AnimationCurve ImageAnimationCurve = null;

    [SerializeField] private Image Background = null;

    [SerializeField] private Color StartColor = Color.clear;
    [SerializeField] private Color EndColor = Color.clear;


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

        var sz = 1+ImageAnimationCurve.Evaluate((float)pct / 100);

        Background.color = Color.Lerp(StartColor, EndColor, sz - 1);
        
        foreach (var rectTransform in ZoomRects)
        {
            rectTransform.localScale = new Vector3(sz, sz, sz);
        }
    }
}
