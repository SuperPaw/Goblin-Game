using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopup : MonoBehaviour
{
    private static AchievementPopup Instance;


    [SerializeField] private GameObject ViewHolder = null;
    [SerializeField] private Image Image = null;
    [SerializeField] private TextMeshProUGUI AchievementName = null;
    [SerializeField] private float Showtime = 4f;

    public void Start()
    {
        if (!Instance) Instance = this;
    }

    public static void ShowAchievement(LegacySystem.Achievement a)
    {
        Instance.StartCoroutine(Instance.ShowAchievementRoutine(a));

    }

    private IEnumerator ShowAchievementRoutine(LegacySystem.Achievement a)
    {
        yield return new WaitUntil(() => !ViewHolder.activeInHierarchy);

        SoundController.PlayStinger(SoundBank.Stinger.AchievementUnlocked);
        
        ViewHolder.SetActive(true);

        Image.sprite = a.Image;

        AchievementName.text = a.Name;

        yield return new WaitForSeconds(Showtime);

        ViewHolder.SetActive(false);
    }
}
