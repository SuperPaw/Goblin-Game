using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldToScreenText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textField;

    private enum floatTextType { Location,Person}

	void Update ()
	{
	    if (!textField)
	    {
	        textField = WorldTextHolder.CreateNewMapEntry();
            
	    }

	    textField.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
	}

    void OnDisable()
    {
        if (textField)
            textField.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if(textField)
            textField.gameObject.SetActive(true);
    }
}
