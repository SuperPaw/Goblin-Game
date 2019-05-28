using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoClick : MonoBehaviour
{
    private static InfoClick Instance;
    public Image IconImage, IconBackgroundImage;
    public TextMeshProUGUI Title, Description;
    public RectTransform Viewholder;
    public int yMin, yMax, xMin, xMax;

    //REct transform is equal the clicked box

    void Start()
    {
        Instance = this;
        Viewholder.gameObject.SetActive(false);
    }

    private void UpdateBounds()
    {

        var res = Screen.currentResolution;

        xMin = -(res.width / 2) + (int)Viewholder.rect.size.x / 2;
        xMax = (res.width / 2) - (int)Viewholder.rect.size.x / 2; ;
        yMin = -res.height / 2 + (int)Viewholder.rect.size.y;
        yMax = res.height / 2;
    }

    public static void ShowInfo(RectTransform clicked, string title, string description, Sprite icon = null)
    {
        Instance.StartCoroutine(Instance.Show(clicked, title, description, icon));
    }

    private IEnumerator Show(RectTransform clicked, string title, string description, Sprite icon = null)
    {
        yield return new WaitForFixedUpdate();

        Viewholder.gameObject.SetActive(true);

        UpdateBounds();

        Viewholder.localPosition = new Vector3(Mathf.Clamp(clicked.localPosition.x, xMin, xMax),
            Mathf.Clamp(clicked.localPosition.y, yMin, yMax), clicked.localPosition.z); 

        Debug.Log(Viewholder.position);

        Title.text = title;
        Description.text = description;
        IconImage.sprite = icon;
        IconBackgroundImage.gameObject.SetActive(icon);

        yield return new WaitForSeconds(0.5f);
        
        yield return new WaitUntil(() => Input.anyKeyDown || Input.touchCount > 0);

        Viewholder.gameObject.SetActive(false);
    }


}
