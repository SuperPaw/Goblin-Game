using System;
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

    public void ShowPopUp()
    {
        var desc = "Unlocks ";
        if (Achievement.UnlocksClass != "")
        {
            desc += "the " + Achievement.UnlocksClass + " class." + System.Environment.NewLine;
        }
        else if (Achievement.UnlocksBlessing != LegacySystem.Blessing.NoBlessing)
        {
            desc += "the " + Achievement.UnlocksBlessing + " blessing." + System.Environment.NewLine;

        }
        else if (Achievement.UnlocksMapSize != MapGenerator.WorldSize.Small)
        {
            desc += "a " + Achievement.UnlocksMapSize + " world." + System.Environment.NewLine;
        }
        if (Achievement.Unlockable && (Achievement.VisibleUnlockCondition || Achievement.Unlocked))
        {
            desc += "Unlocked by ";
            switch (Achievement.Condition)
            {
                case LegacySystem.UnlockCondition.KillGiant:
                    desc += "killing " + Achievement.AmountToUnlock + " giant.";
                    break;
                case LegacySystem.UnlockCondition.KillSpider:
                    desc += "killing " + Achievement.AmountToUnlock + " spiders.";
                    break;
                case LegacySystem.UnlockCondition.KillZombie:
                    desc += "killing " + Achievement.AmountToUnlock + " zombie goblins.";
                    break;
                case LegacySystem.UnlockCondition.KillHuman:
                    desc += "killing " + Achievement.AmountToUnlock + " man.";
                    break;
                case LegacySystem.UnlockCondition.KillWolf:
                    desc += "killing " + Achievement.AmountToUnlock + " wolfs.";
                    break;
                case LegacySystem.UnlockCondition.EquipmentFound:
                    desc += "finding " + Achievement.AmountToUnlock + " equipment.";
                    break;
                case LegacySystem.UnlockCondition.GoblinSacrifice:
                    desc += "sacrificing " + Achievement.AmountToUnlock + " goblins.";
                    break;
                case LegacySystem.UnlockCondition.GoblinDeath:
                    desc += "having " + Achievement.AmountToUnlock + " goblins die.";
                    break;
                case LegacySystem.UnlockCondition.Treasure:
                    desc += "finding " + Achievement.AmountToUnlock + " goblin treasure.";
                    break;
                case LegacySystem.UnlockCondition.DestroyFarm:
                    desc += "destroying " + Achievement.AmountToUnlock + " man farm.";
                    break;
                case LegacySystem.UnlockCondition.FindANewHome:
                    desc += "finding a new home";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        InfoClick.ShowInfo(AchievementImage.rectTransform,Achievement.Name,desc,Achievement.Image);
    }
}
