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
    public AiryUIAnimationManager ViewHolder;

    public struct ShowEvent
    {
        public string Text;
        public Transform Trans;

        public ShowEvent(string text, Transform trans)
        {
            Text = text;
            Trans = trans;
        }
    }

    public Queue<ShowEvent> TextToShow = new Queue<ShowEvent>();
    public static bool ShowingText;
    public float ShowTime = 4;
    //private AiryUIAnimatedElement comp;

    // Start is called before the first frame update
    void Awake()
    {
        if (!Instance)
            Instance = this;

        //comp = GetComponentInChildren<AiryUIAnimatedElement>();

        ViewHolder.SetActive(false);

        PopText.text = "";
    }

    void FixedUpdate()
    {
        if (TextToShow.Any() && !ShowingText && GameManager.Instance.GameStarted)
            StartCoroutine(ShowTextLoop(TextToShow.Dequeue()));
    }

    public static void ShowText(string text, Transform trans)
    {
        Instance.TextToShow.Enqueue(new ShowEvent(text,trans));
    }

    private IEnumerator ShowTextLoop(ShowEvent showing)
    {
        ShowingText = true;
        
        PopText.text = showing.Text;

        SoundController.PlayEvent();
        
        ViewHolder.gameObject.SetActive(true);
        //comp.ShowElement();

        //TODO: slow down time
        //GameManager.Pause();
        
        //TODO: different sizes for different types of events

        var endTime = Time.unscaledTime + ShowTime;

        while (Time.unscaledTime < endTime)// & !Input.anyKeyDown && Input.touchCount == 0)
        {
            PlayerController.MoveCameraToPos(showing.Trans.position, 6);
            yield return null;
        }

        ViewHolder.SetActive(false);
        //GameManager.UnPause();

        yield return new WaitUntil(() => ViewHolder.AllHidden());
       
        //TODO: only unpause if it was already unpaused

        ShowingText = false;

        //PopText.text = "";
    }
}
