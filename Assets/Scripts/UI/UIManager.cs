using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GUIStyle HoverStyle;
    public GUIStyle HoverStyleBack;

    [Header("Highlight Text")]
    public float HighlightTime;
    public AnimationCurve HighlightAnimation;
    public Color HighlightColor;
    public float HighlightSizeIncrease;

    public List<TextMeshProUGUI> Animating = new List<TextMeshProUGUI>();

    void Start()
    {
        if (!Instance)
            Instance = this;
    }


    public static void HighlightText(TextMeshProUGUI text)
    {
        if(!Instance.Animating.Contains(text))
            Instance.StartCoroutine(Instance.HighlightRoutine(text));
    }

    //TODO: include particle effects
    private IEnumerator HighlightRoutine(TextMeshProUGUI text)
    {
        Animating.Add(text);

        var rect = text.rectTransform;

        yield return null;

        var startTime = Time.unscaledTime;
        var endtime = Time.unscaledTime + HighlightTime;
        var startScale = rect.localScale;
        var startColor = text.color;

        while (endtime> Time.unscaledTime)
        {
            yield return null;
            var t = HighlightAnimation.Evaluate((Time.unscaledTime - startTime) / HighlightTime);

            rect.localScale = startScale * (1 + t * HighlightSizeIncrease) ;

            text.color = Color.Lerp(startColor,HighlightColor,t);
        }

        Animating.Remove(text);
    }
}
