using System.Collections;
using System.Collections.Generic;
using BayatGames.SaveGameFree;
using TMPro;
using UnityEngine;

public class SaveTest : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI TestText;

    // Start is called before the first frame update
    void Start()
    {
        if (SaveController.SaveTest())
        {
            TestText.text = "Save test PASSED";
            TestText.color = Color.green;
        }
        else
        {
            TestText.text = "Save test FAILED";
            TestText.color = Color.red;

        }
    }
    
}
