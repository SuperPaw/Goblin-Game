using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpText : MonoBehaviour
{
    public RectTransform Rect;
    public TextMeshProUGUI PopText;
    private static PopUpText Instance;
    public AiryUIAnimationManager ViewHolder;
    public GameObject ImageHolder;
    public Image MessageImage;

    public struct ShowEvent
    {
        public string Text;
        public Transform Trans;
        public Sprite Sprite;

        public ShowEvent(string text, Transform trans, Sprite sprite)
        {
            Text = text;
            Trans = trans;
            Sprite = sprite;
        }
    }

    public Queue<ShowEvent> TextToShow = new Queue<ShowEvent>();
    public static bool ShowingText;
    public float ShowTime = 5;
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

    public static void ShowText(string text, Transform trans, Sprite messageSprite)
    {
        Instance.TextToShow.Enqueue(new ShowEvent(text,trans,messageSprite));
    }

    private IEnumerator ShowTextLoop(ShowEvent showing)
    {
        ShowingText = true;
        
        PopText.text = showing.Text;
        ImageHolder.SetActive(showing.Sprite);
        MessageImage.sprite = showing.Sprite;

        SoundController.PlayEvent();
        
        ViewHolder.gameObject.SetActive(true);
        //comp.ShowElement();

        //TODO: slow down time
        //GameManager.Pause();

        //TODO: different sizes for different types of events
        var endTime = Time.unscaledTime + ShowTime;

        //CHECK IF EVENT IS WITHIN FOG OF WAR
        var chiefLocation = PlayerController.Instance.Team.Leader.transform.position;
        chiefLocation.y = 0;
        var showingY0 = showing.Trans.position;

        if(PlayerController.ObjectIsSeen(showing.Trans))
        {
            while (Time.unscaledTime < endTime & !Input.anyKeyDown && Input.touchCount == 0 && (Math.Abs(Input.mouseScrollDelta.y) < 0.001))
            {
                PlayerController.MoveCameraToPos(showing.Trans.position, 6);
                yield return null;
            }
        }
        else
            Debug.Log("Popup too far away to show!");

        yield return new WaitUntil(() => Time.unscaledTime > endTime);


        ViewHolder.SetActive(false);
        //GameManager.UnPause();

        yield return new WaitForSeconds(1f);
        
        //TODO: only unpause if it was already unpaused

        ShowingText = false;

        //PopText.text = "";
    }
}
