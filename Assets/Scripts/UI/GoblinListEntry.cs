using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoblinListEntry : MonoBehaviour
{
    public Image ClassImage;
    public TextMeshProUGUI NameText;
    public Goblin Goblin;
    public Image LevelUpReady, ChiefImage;
    public Sprite DeadGoblinSprite;

    public void SelectGoblin()
    {
        CharacterView.ShowCharacter(Goblin);
    }

    public void MarkAsDead()
    {
        if(ClassImage) ClassImage.sprite = DeadGoblinSprite;
    }
}
