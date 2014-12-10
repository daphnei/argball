using UnityEngine;
using System.Collections;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

public class TesterCamera : MonoBehaviour {

	public CustomCamera custom;

	public Vector2[] mid;
	public Vector2[] final;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.camera.projectionMatrix = custom.arCamera.projectionMatrix;
		custom.points = custom.trackers.Select(tracker => {
			Vector3 p = this.camera.WorldToScreenPoint(tracker.transform.position);
			return new Vector2(p.x, p.y);
		}).ToArray();

		if (custom.homography != null) {
			Matrix4x4 worldToProjection = this.camera.projectionMatrix * this.camera.worldToCameraMatrix;
			//Matrix4x4 projectionToOther = custom.homography.Inverse().ToMatrix4x4();
			//Matrix4x4 worldToOther = projectionToOther * worldToProjection;
			Matrix inv = custom.homography.Inverse();
			mid = custom.trackers.Select(tracker => {
				Vector3 p = worldToProjection.MultiplyPoint(tracker.transform.position); // * new Vector4(tracker.transform.position.x, tracker.transform.position.y, tracker.transform.position.z, 1); 
				float winx = ((p.x + 1) / 2) * this.camera.pixelWidth;
				float winy = ((p.y + 1) / 2) * this.camera.pixelHeight;
				return new Vector2(winx, winy);
			}).ToArray();
			final = mid.Select(v => {
				Matrix f = inv * v.ToHomogeneousVector().ToColumnMatrix();
				Vector3 vf = f.GetColumnVector(0).ToVector3();
				return new Vector2(vf.x, vf.y) / vf.z;
			}).ToArray();
		}
	}

	void OnGUI() {
		if (final != null) {
			for (int i = 0; i < final.Length; i++) {
				GUI.color = Color.green;
				GUI.Label(new Rect(final[i].x - 2, Screen.height - final[i].y - 2, 40, 40), "O");
				GUI.color = Color.blue;
				GUI.Label(new Rect(mid[i].x - 2, mid[i].y - 2, 40, 40), "O");
				GUI.color = Color.cyan;
				GUI.Label(new Rect(custom.points[i].x - 7, custom.points[i].y - 7, 40, 40), "O");
			}
		}
	}
}