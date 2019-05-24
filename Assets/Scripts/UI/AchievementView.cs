using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementView : MonoBehaviour
{
    private static AchievementView Instance;

    [SerializeField]
    private GameObject ViewHolder = null;

    [SerializeField] private AchievementEntry Entry;

    private List<GameObject> InstantiatedObjects = new List<GameObject>();

    void Start()
    {
        if (!Instance)
            Instance = this;
    }

    public void Close()
    {
        ViewHolder.SetActive(false);
    }

    public static void OpenView()
    {
        Instance.SetupView();
    }

    public void ResetLegacy()
    {
        LegacySystem.ResetAchievements();

        LoadingScreen.ResetLegacyMenu();
        SetupView();
    }

    private void SetupView()
    {
        foreach (var i in InstantiatedObjects)
        {
            Destroy(i);
        }
        InstantiatedObjects.Clear();

        var achs = LegacySystem.GetAchievements();

        foreach (var a in achs)
        {
            var entry = Instantiate(Entry, Entry.transform.parent);
            entry.gameObject.SetActive(true);
            entry.SetupAchievement(a);
            InstantiatedObjects.Add(entry.gameObject);
        }
        Entry.gameObject.SetActive(false);

        ViewHolder.SetActive(true);
    }
}
