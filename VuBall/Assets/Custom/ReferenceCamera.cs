using UnityEngine;
using System.Collections;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// A reference camera to represent the ground-truth camera for calculating homographies.
/// Normally, these extrinisc parameters are hardcoded, and the intrinsic parameters are 
/// calculated.
/// </summary>
public class ReferenceCamera : MonoBehaviour {

	// The camera for which we set our custom points.
	public CustomCamera customCamera;

	public Vector2[] markers1;
	public Vector2[] markers2;

	/// <summary>
	/// Uses the known information about the camera extrinsics and intrinsics to build a
	/// homography from the world to the other.
	/// </summary>
	/// <param name="homographyToMyScreen"></param>
	/// <returns></returns>
	public Matrix4x4 GetTransformFromWorldToOther(Matrix homographyToMyScreen) {
		Matrix4x4 homoInv = homographyToMyScreen.Inverse().ToMatrix4x4();
		return homoInv * this.GetCameraIntrinsicsMatrix() * this.camera.worldToCameraMatrix;
	}

	public Matrix4x4 GetCameraIntrinsicsMatrix() {
		Matrix4x4 projectionToScreen = new Matrix4x4();
		projectionToScreen.m00 = projectionToScreen.m02 = this.camera.pixelWidth  * 0.5f;
		projectionToScreen.m11 = projectionToScreen.m12 = this.camera.pixelHeight  * 0.5f;
		projectionToScreen.m22 = 1f;
		projectionToScreen.m33 = 1f;
		Matrix4x4 proj = this.camera.projectionMatrix;
		//proj.m32 = 0f;
		//proj.m23 = 0f;
		return projectionToScreen * proj;
	}
	
	// Update is called once per frame
	void Update () {

		// Grab the projection matrix from thr AR camera, then generate reference points.
		this.camera.projectionMatrix = this.customCamera.arCamera.projectionMatrix;
		customCamera.points = customCamera.trackers.Select(tracker => {
			Vector3 p = this.camera.WorldToScreenPoint(tracker.transform.position);
			return new Vector2(p.x, p.y);
		}).ToArray();

		if (this.customCamera.homography != null) {
			Matrix4x4 complete = this.GetTransformFromWorldToOther(this.customCamera.homography);
			this.markers1 = customCamera.trackers.Select(tracker => {
				Vector3 t = tracker.transform.position;
				Vector3 p = complete * new Vector4(t.x, t.y, t.z, 1); 
				return new Vector2(p.x, p.y) / p.z;
			}).ToArray();
		}
	}

	void OnGUI() {
		if (!CustomCamera.CustomDebug) {
			return;
		}
		if (this.markers1 != null) {
			for (int i = 0; i < markers1.Length; i++) {
				GUI.color = Color.blue;
				GUI.Label(new Rect(markers1[i].x - 2, Screen.height - markers1[i].y - 2, 40, 40), "O");
			}
		}		
		if (this.markers2 != null) {
			for (int i = 0; i < markers2.Length; i++) {
				GUI.color = Color.green;
				GUI.Label(new Rect(markers2[i].x - 2, Screen.height - markers2[i].y - 2, 40, 40), "O");
				GUI.color = Color.cyan;
				GUI.Label(new Rect(customCamera.points[i].x - 7, customCamera.points[i].y - 7, 40, 40), "O");
			}
		}
		if (this.customCamera.points != null) {
			for (int i = 0; i < customCamera.points.Length; i++) {
				GUI.color = Color.cyan;
				GUI.Label(new Rect(customCamera.points[i].x - 7, customCamera.points[i].y - 7, 40, 40), "O");
			}
		}
	}
}