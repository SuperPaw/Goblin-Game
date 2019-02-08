using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MenuWindow : MonoBehaviour
{
    public GameObject ViewHolder;

    public enum WindowType
    {
        Character,PlayerChoice, LocationView ,
        COUNT
    }

    public WindowType Type;

    //public static Queue<WindowType> WaitingToOpen = new Queue<WindowType>();

    public static Dictionary<WindowType, bool> OpenWindows = new Dictionary<WindowType, bool>();

    protected void Start()
    {
        for (int i = 0; i < (int)WindowType.COUNT; i++)
        {
            OpenWindows[(WindowType) i] = false;
        }
    }

    protected void Open()
    {
        SoundController.PlayMenuPopup();

        ViewHolder.SetActive(true);
        //GameManager.Pause();
        OpenWindows[Type] = true;
    }

    protected void Close()
    {
        ViewHolder.SetActive(false);
        OpenWindows[Type] = false;

        if (!OpenWindows.Values.Any(open => open))
        {
            //GameManager.UnPause();
        }
    }
}
