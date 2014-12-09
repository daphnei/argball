using UnityEngine;
using System.Collections;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Runtime.InteropServices;


public class CustomCamera : MonoBehaviour {

	public Camera arCamera;
	public GameObject[] trackers;
	public Vector2[] points;

	public float focalLength = 482.0234f;
	public Vector2 cameraCenter = new Vector2(225.0186f, 140.0390f);

	// Update is called once per frame
	void Update () {
		Vector2[] screenPoints = trackers.Select(obj => {
			Vector3 screen = this.arCamera.WorldToScreenPoint(obj.transform.position);
			return new Vector2(screen.x, screen.y);
		}).ToArray();

		Matrix homography = MathSupport.ComputeHomography(screenPoints, points);		
		Matrix pose = Matrix.Identity(3, 4);

		// Set the first two columns of the pose.
		pose.SetColumnVector(homography.GetColumnVector(0).Normalize(), 0);
		pose.SetColumnVector(homography.GetColumnVector(1).Normalize(), 1);

		// Set the third column as the cross of the first two.
		Vector cross = pose.GetColumnVector(0).CrossMultiply(pose.GetColumnVector(1));
		pose.SetColumnVector(cross, 2);

		// Set the forth column of the pose matrix.
		float norm1 = (float)homography.GetColumnVector(0).Norm();
		float norm2 = (float)homography.GetColumnVector(1).Norm();
		float tnorm = (norm1 + norm2) / 2;
		pose.SetColumnVector(homography.GetColumnVector(2) / tnorm, 3);

		// Create an intrinsics matrix.
		Matrix intrinsics = new Matrix(3, 3);
		intrinsics[0, 0] = this.focalLength;
		intrinsics[1, 1] = this.focalLength;
		intrinsics[0, 2] = this.cameraCenter.x;
		intrinsics[1, 2] = this.cameraCenter.y;
		intrinsics[2, 2] = 1;

		Matrix extrinsics = intrinsics.Inverse() * pose;

		Matrix camRotation = extrinsics.GetMatrix(0, 2, 0, 2);
		Vector camTranslation = extrinsics.GetColumnVector(3);

		Matrix actualTranslation = -1 * Matrix.Transpose(camRotation) * camTranslation.ToColumnMatrix();
		this.transform.localPosition = actualTranslation.GetColumnVector(0).ToVector3();
		this.transform.localRotation = MathSupport.ConvertRotationMatrixToQuaternion(camRotation);
		this.camera.projectionMatrix = this.arCamera.projectionMatrix;

		float focalLength = 0.5f * this.arCamera.pixelHeight / ((float)Math.Tan(Mathf.Deg2Rad * this.arCamera.fieldOfView / 2));
		Debug.Log(this.camera.projectionMatrix + "FOCAL" + focalLength);
		Debug.Log(CameraDevice.Instance.ToString());
	}
}
