
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen Instance;
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
    [SerializeField] private Button LegacyButton = null;

    [SerializeField] private GameObject WorldSizeTextHolder = null;
    [SerializeField] private GameObject ClassSelectHolder = null;
    [SerializeField] private GameObject TribeSelectHolder = null;

    [SerializeField] private TMP_Dropdown WorldSizeSelect = null;
    [SerializeField] private TMP_Dropdown ChiefClassSelect = null;
    [SerializeField] private TMP_Dropdown TribeBlessingSelect = null;


    public void Start()
    {
        if (!Instance) Instance = this;

        SetupLegacyMenu();

        LegacyButton.onClick.AddListener(AchievementView.OpenView);

        MapGen.SetSize(MapGenerator.WorldSize.Small);
    }

    public void StartGame()
    {
        //could load different scenes instead of just running the generate

        SoundController.ChangeMusic(SoundBank.Music.Menu);

        StartButton.interactable = false;

        Destroy(WorldSizeTextHolder);
        Destroy(ClassSelectHolder);
        Destroy(TribeSelectHolder);
        Destroy(LegacyButton.gameObject);
        MapGen.gameObject.SetActive(true);

        StartCoroutine(MapGen.GenerateMap(SetLoadingText, () => Destroy(gameObject)));
        //TODO: include gobbo creation in loading
    }

    public static void ResetLegacyMenu()
    {
        Instance.SetupLegacyMenu();
    }

    private void SetupLegacyMenu()
    {
        var legs = LegacySystem.GetAchievements();

        // CLASS SELECT SET-UP
        if (legs.Any(a => a.Unlocked && a.UnlocksClass != ""))
        {
            ClassSelectHolder.SetActive(true);

            var unlocked = legs.Where(e => e.Unlocked && e.UnlocksClass != "").Select(a => a.UnlocksClass).Distinct().OrderBy(a => a);

            ChiefClassSelect.ClearOptions();

            ChiefClassSelect.AddOptions(new List<TMP_Dropdown.OptionData>() { new TMP_Dropdown.OptionData("No class") });

            ChiefClassSelect.AddOptions(unlocked.Select(u => new TMP_Dropdown.OptionData(u.ToString())).ToList());
        }
        else
        {
            ClassSelectHolder.SetActive(false);
        }

        // World SELECT SET-UP
        if (legs.Any(a => a.Unlocked && a.UnlocksMapSize != MapGenerator.WorldSize.Small))
        {
            WorldSizeTextHolder.SetActive(true);

            var unlocked = legs.Where(e => e.Unlocked && e.UnlocksMapSize != MapGenerator.WorldSize.Small).Select(a => a.UnlocksMapSize).Distinct().OrderBy(a => a);

            WorldSizeSelect.ClearOptions();

            WorldSizeSelect.AddOptions(new List<TMP_Dropdown.OptionData>() { new TMP_Dropdown.OptionData("Small") });

            WorldSizeSelect.AddOptions(unlocked.Select(u => new TMP_Dropdown.OptionData(u.ToString())).ToList());
        }
        else
        {
            WorldSizeTextHolder.SetActive(false);
        }

        // Blessing SELECT SET-UP
        if (legs.Any(a => a.Unlocked && a.UnlocksBlessing != LegacySystem.Blessing.NoBlessing))
        {
            TribeSelectHolder.SetActive(true);

            var unlocked = legs.Where(e => e.Unlocked && e.UnlocksBlessing != LegacySystem.Blessing.NoBlessing).Select(a => a.UnlocksBlessing).Distinct().OrderBy(a => a);

            TribeBlessingSelect.ClearOptions();

            TribeBlessingSelect.AddOptions(new List<TMP_Dropdown.OptionData>() { new TMP_Dropdown.OptionData("No blessing") });

            TribeBlessingSelect.AddOptions(unlocked.Select(u => new TMP_Dropdown.OptionData(u.ToString())).ToList());
        }
        else
        {
            TribeSelectHolder.SetActive(false);
        }

        LegacyButton.gameObject.SetActive(TribeSelectHolder.activeInHierarchy || ClassSelectHolder.activeInHierarchy ||
                                          WorldSizeTextHolder.activeInHierarchy);
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
        Debug.Log("Selecting world size: " + WorldSizeSelect.options[sz].text);

        MapGen.SetSize((MapGenerator.WorldSize)Enum.Parse(typeof(MapGenerator.WorldSize), WorldSizeSelect.options[sz].text));
    }

    public void SelectChiefClass(int c)
    {
        Debug.Log("Selecting class: " + ChiefClassSelect.options[c].text);

        PlayerTeam.LeaderClass = (Goblin.Class)Enum.Parse(typeof(Goblin.Class),ChiefClassSelect.options[c].text);
    }

    public void SelectTribeBlessing(int c)
    {
        Debug.Log("Selecting blessing: " + TribeBlessingSelect.options[c].text);

        PlayerTeam.TribeBlessing = (LegacySystem.Blessing)Enum.Parse(typeof(LegacySystem.Blessing), TribeBlessingSelect.options[c].text);
    }
}
