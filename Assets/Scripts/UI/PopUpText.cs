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
        public Vector3? Position;

        public ShowEvent(string text, Vector3? position)
        {
            Text = text;
            Position = position;
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

    public static void ShowText(string text, Vector3 position)
    {
        Instance.TextToShow.Enqueue(new ShowEvent(text,position));
    }

    private IEnumerator ShowTextLoop(ShowEvent showing)
    {
        ShowingText = true;
        
        PopText.text = showing.Text;

        SoundController.PlayEvent();
        
        ViewHolder.gameObject.SetActive(true);
        //comp.ShowElement();

        //TODO: slow down time
        GameManager.Pause();
        
        //TODO: different sizes for different types of events
        PlayerController.MoveCameraToPos(showing.Position.Value,6);

        var endTime = Time.unscaledTime + ShowTime;

        yield return new WaitForSecondsRealtime(0.5f);

        yield return new WaitUntil(() => Time.unscaledTime > endTime || Input.anyKeyDown || Input.touchCount > 0);

        ViewHolder.SetActive(false);
        GameManager.UnPause();

        yield return new WaitUntil(() => ViewHolder.AllHidden());
       
        //TODO: only unpause if it was already unpaused

        ShowingText = false;

        //PopText.text = "";
    }
}
