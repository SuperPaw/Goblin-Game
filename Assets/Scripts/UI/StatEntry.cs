﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatEntry : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Value;
    public OnValueHover ValueHover;
    public Image FillImage;
    public Color LowStatColor;
    public Color HighStatColor;
}
