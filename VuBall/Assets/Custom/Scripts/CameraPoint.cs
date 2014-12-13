using UnityEngine;
using System.Collections;

/// <Summary>
/// Represents one of the four corners of the reference image within the current
/// frame, as it is detected by Vuforia. If the debug flag is on, this corner
/// is visualized by a red dot.
/// </Summary>
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
