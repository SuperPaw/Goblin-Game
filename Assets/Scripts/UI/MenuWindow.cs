using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public abstract class MenuWindow : MonoBehaviour
{
    [NotNull]
    public AiryUIAnimationManager ViewHolder;

    public enum WindowType
    {
        Character,PlayerChoice, LocationView ,LevelUp,
        COUNT
    }

    public WindowType Type;

    //public static Queue<WindowType> WaitingToOpen = new Queue<WindowType>();

    public static Dictionary<WindowType, bool> OpenWindows = new Dictionary<WindowType, bool>();

    protected void Awake()
    {
        for (int i = 0; i < (int)WindowType.COUNT; i++)
        {
            OpenWindows[(WindowType) i] = false;
        }
        
        PlayerController.OnZoomLevelChange.AddListener(v=> Close());
    }

    protected void Open()
    {
        SoundController.PlayMenuPopup();

        ViewHolder.SetActive(true);
        //GameManager.Pause();
        OpenWindows[Type] = true;
    }

    public void Close()
    {
        //Debug.Log("Closing view: " +Type);

        ViewHolder.SetActive(false);
        OpenWindows[Type] = false;

        if (!OpenWindows.Values.Any(open => open))
        {
            //GameManager.UnPause();
        }

        //PlayerController.FollowGoblin = null;
    }
}
