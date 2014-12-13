using UnityEngine;
using System.Collections;

/// <summary>
/// Debug marker.
/// </summary>
public class DebugMarker : MonoBehaviour {
	void Update() {
		this.renderer.enabled = CustomCamera.CustomDebug;
	}
}
