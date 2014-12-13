using UnityEngine;
using System.Collections;

public class DebugMarker : MonoBehaviour {
	void Update() {
		this.renderer.enabled = CustomCamera.CustomDebug;
	}
}
