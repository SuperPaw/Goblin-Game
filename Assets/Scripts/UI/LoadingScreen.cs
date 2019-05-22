
using System;
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

    [SerializeField] private Button StartButton = null;

    [SerializeField] private GameObject WorldSizeTextHolder = null;
    [SerializeField] private GameObject ClassSelectHolder = null;
    [SerializeField] private GameObject TribeSelectHolder = null;

    [SerializeField] private TMP_Dropdown WorldSizeSelect = null;
    [SerializeField] private TMP_Dropdown ChiefClassSelect = null;
    [SerializeField] private TMP_Dropdown TribeBlessingSelect = null;


    public void Start()
    {
        var legs = LegacySystem.GetAchievements();

        // CLASS SELECT SET-UP
        if (legs.Any(a => a.Unlocked && a.UnlocksClass != Goblin.Class.NoClass))
        {
            var unlocked = legs.Where(e => e.Unlocked).Select(a => a.UnlocksClass).Distinct();

            ChiefClassSelect.ClearOptions();

            ChiefClassSelect.AddOptions(unlocked.Select(u => new TMP_Dropdown.OptionData(u.ToString())).ToList());
        }
        else
        {
            ClassSelectHolder.SetActive(false);
        }

        // World SELECT SET-UP
        if (legs.Any(a => a.Unlocked && a.UnlocksMapSize != MapGenerator.WorldSize.Small))
        {
            var unlocked = legs.Where(e => e.Unlocked).Select(a => a.UnlocksMapSize).Distinct();

            WorldSizeSelect.ClearOptions();

            WorldSizeSelect.AddOptions(unlocked.Select( u => new TMP_Dropdown.OptionData(u.ToString())).ToList());
        }
        else
        {
            WorldSizeTextHolder.SetActive(false);
        }

        // Blessing SELECT SET-UP
        if (legs.Any(a => a.Unlocked && a.UnlocksBlessing != LegacySystem.Blessing.NoBlessing))
        {
            var unlocked = legs.Where(e=>e.Unlocked).Select(a => a.UnlocksBlessing).Distinct();

            WorldSizeSelect.ClearOptions();

            WorldSizeSelect.AddOptions(unlocked.Select(u => new TMP_Dropdown.OptionData(u.ToString())).ToList());
        }
        else
        {
            TribeSelectHolder.SetActive(false);
        }
    }


    public void StartGame()
    {
        //could load different scenes instead of just running the generate

        SoundController.ChangeMusic(SoundBank.Music.Menu);

        StartButton.interactable = false;
        Destroy(WorldSizeTextHolder);
        Destroy(ClassSelectHolder);
        Destroy(TribeSelectHolder);
        MapGen.gameObject.SetActive(true);

        StartCoroutine(MapGen.GenerateMap(SetLoadingText, ()=>Destroy(gameObject)));
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

    public void SelectWorldSize(int sz)
    {
        MapGen.SetSize((MapGenerator.WorldSize) sz);
    }
}
