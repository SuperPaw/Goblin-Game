using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementEntry : MonoBehaviour
{
    [SerializeField]
    private LegacySystem.Achievement Achievement = null;
    [SerializeField]
    private Image AchievementImage = null;
    [SerializeField] private Color UnlockedColor = Color.white;
    [SerializeField] private Color LockedColor = Color.grey;

    public void SetupAchievement(LegacySystem.Achievement a)
    {
        Achievement = a;
        AchievementImage.sprite = a.Image;
        AchievementImage.color = a.Unlocked ? UnlockedColor : LockedColor;
    }
}
