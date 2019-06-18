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
    private AiryUIAnimatedElement comp;

    // Start is called before the first frame update
    void Awake()
    {
        if (!Instance)
            Instance = this;

        comp = GetComponentInChildren<AiryUIAnimatedElement>();

        PopText.text = "";
    }

    void FixedUpdate()
    {
        if (TextToShow.Any() && !ShowingText && GameManager.Instance.GameStarted)
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
        
        comp.gameObject.SetActive(true);
        comp.ShowElement();
        
        yield return new WaitForSeconds(ShowTime);

        comp.HideElement();

        //Debug.Log("*hiding elements");
        yield return new WaitUntil(() => !comp.gameObject.activeInHierarchy);

        //Debug.Log(" elements are animated awaya");

        ShowingText = false;

        //PopText.text = "";
    }
}
