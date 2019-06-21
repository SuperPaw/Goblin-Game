using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(AiryUIAnimatedElement))]
public class AiryUIAnimationManager : MonoBehaviour
{
    [HideInInspector] public AiryUIAnimatedElement[] childrenElements;
    [Tooltip("Wheather or not to show the animation when the menu is enabled")] public bool showMenuOnEnable = true;

    private bool elementsUpdated = false;

    public bool Active;

    private void Awake()
    {
        elementsUpdated = false;
        if (!elementsUpdated)
        {
            UpdateElementsInChildren();
        }
    }

    private void OnEnable()
    {
        if (showMenuOnEnable && elementsUpdated)
        {
            //if(DynamicChildrenCount)
            //UpdateElementsInChildren();

            ShowMenu();
        }
    }

    public void SetActive(bool enable)
    {
        if(enable == Active)
            return;

        if(enable)
            ShowMenu();
        else
            HideMenu();

    }

    public void ShowMenu()
    {
        gameObject.SetActive(true);
        Active = true;

        if (elementsUpdated)
        {
            foreach (var element in childrenElements)
            {
                if (!element) continue;

                if (element.showItemOnMenuEnable)
                    element.ShowElement();
            }
        }
    }

    public void HideMenu()
    {
        foreach (var element in childrenElements)
        {
            if(!element || !element.isActiveAndEnabled) continue;
            element.HideElement();
            element.OnHideComplete.AddListener(() =>
                Active = false);
        }
    }

    public void UpdateElementsInChildren()
    {
        childrenElements = GetComponentsInChildren<AiryUIAnimatedElement>();
        elementsUpdated = true;
    }

    public bool AllHidden()
    {
        return childrenElements.All(e => e.Hidden);
    }
    public bool ThisHidden()
    {
        return GetComponent<AiryUIAnimatedElement>().Hidden;
    }
}