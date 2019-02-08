using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackIconSelector : MonoBehaviour
{
    public Image Button;
    public Sprite AttackIcon, AmbushIcon;
    public PlayerTeam GoblinTeam;

    // Update is called once per frame
    void FixedUpdate()
    {
        Button.sprite = GoblinTeam.AllHidden() ? AmbushIcon : AttackIcon;
    }
}
