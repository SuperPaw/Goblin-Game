using UnityEngine;

public class WorldToScreenSpace : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    private Quaternion rotation;

    void Start()
    {
        rotation = transform.rotation;
        //rotation.y = -45;
    }


	void Update ()
	{
	    //if (!canvas)
	    //{
	    //    canvas = Singleton<WorldTextHolder>.Instance.Unwrap().CreateNewMapEntry();

	    //    if (type == floatTextType.Location)
	    //        canvas.text = GetComponentInParent<Location>().GetName();
	    //}
	    transform.rotation = rotation;
	    //canvas.transform.position = Camera.main.WorldToScreenPoint(this.transform.position);
	}

    //void OnDisable()
    //{
    //    if (canvas)
    //        canvas.gameObject.SetActive(false);
    //}

    //void OnEnable()
    //{
    //    if(canvas)
    //        canvas.gameObject.SetActive(true);
    //}
}
