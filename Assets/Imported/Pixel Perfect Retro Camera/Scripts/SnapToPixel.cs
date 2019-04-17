using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SnapToPixel : MonoBehaviour {
	public PixelCamera cam;
	
	float d;
	
	void Start() {
		//cam = GetComponent<PixelCamera>();
		
		d = 1f / cam.pixelsPerUnit;
	}

	void LateUpdate() {
		Vector3 pos = transform.position;
		Vector3 camPos = new Vector3 (pos.x - pos.x % d, pos.y - pos.y % d, pos.z);	
		cam.transform.position = camPos;
	}
}
