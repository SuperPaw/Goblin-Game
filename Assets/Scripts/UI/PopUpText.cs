using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PopUpText : MonoBehaviour
{
    public RectTransform Rect;
    public TextMeshProUGUI PopText;
    private static PopUpText Instance;
    public Queue<string> TextToShow = new Queue<string>();
    private bool ShowingText;
    public float ShowTime = 4;

    // Start is called before the first frame update
    void Start()
    {
        if (!Instance)
            Instance = this;

        PopText.text = "";
    }

    void FixedUpdate()
    {
        if (TextToShow.Any() && !ShowingText)
            StartCoroutine(ShowTextLoop(TextToShow.Dequeue()));
    }

    public static void ShowText(string text)
    {
        Instance.TextToShow.Enqueue(text);
    }

    private IEnumerator ShowTextLoop(string text)
    {
        ShowingText = true;

        PopText.text = text;

        SoundController.PlayEvent();

        yield return new WaitForSeconds(ShowTime);

        //TODO: make some animation

        ShowingText = false;

        PopText.text = "";
    }
}
