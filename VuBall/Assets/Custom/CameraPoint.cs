using UnityEngine;
using System.Collections;

public class CameraPoint : MonoBehaviour {

	public Camera attachedCamera;

	/// <summary>
	/// Show markers to represent the GUI.
	/// </summary>
	void OnGUI() {
		if (CustomCamera.CustomDebug) {
			Vector3 screenPos = this.attachedCamera.WorldToScreenPoint(this.transform.position);
			GUI.color = Color.red;
			GUI.Label(new Rect(screenPos.x - 5, Screen.height - screenPos.y - 5, 40, 40), "O");
		}
	}
}
