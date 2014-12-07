using UnityEngine;
using System.Collections;

public class CameraPoint : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI() {
		Vector3 screenPos = Camera.main.WorldToScreenPoint(this.transform.position);
		GUI.Label(new Rect(screenPos.x, Screen.height-screenPos.y, 100, 100), "P");
	}
}
