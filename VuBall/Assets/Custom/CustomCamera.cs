using UnityEngine;
using System.Collections;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;


public class CustomCamera : MonoBehaviour {

	public static bool CustomPlacement = true;
	public static bool CustomDebug = true;

	public Camera arCamera;
	public GameObject[] trackers;
	public Vector2[] points;

	public float focalLength = 1;
	public Vector2 cameraCenter = new Vector2(0, 0);

	// Update is called once per frame
	void Update () {
		this.camera.projectionMatrix = this.arCamera.projectionMatrix;

		Vector3 newPosition;
		Quaternion newRotation;
		if (CustomCamera.CustomPlacement) {
			this.PlaceCameraBasedOnHomography(out newPosition, out newRotation);
		} else {
			this.PlaceCameraBasedOnCheating(out newPosition, out newRotation);
		}

		this.transform.position = newPosition;
		this.transform.rotation = newRotation;
	}

	/// <summary>
	/// Places this camera based on the input points from the other camera.
	/// </summary>
	void PlaceCameraBasedOnHomography(out Vector3 position, out Quaternion rotation) {
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
		Vector cross = pose.GetColumnVector(0).CrossMultiply(pose.GetColumnVector(1)).Normalize();
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
		camRotation.Inverse();
		Matrix camTranslation = extrinsics.GetColumnVector(3).ToColumnMatrix();
		camTranslation = -1 * camRotation * camTranslation;

		position = this.transform.parent.position + camTranslation.GetColumnVector(0).ToVector3();
		rotation = this.transform.parent.rotation * MathSupport.ConvertRotationMatrixToQuaternion(camRotation);

		// ATTEMPT AT CALCULATING THE FOCAL LENGTH
		// float focalLength = 0.5f * this.arCamera.pixelHeight / ((float)Math.Tan(Mathf.Deg2Rad * this.arCamera.fieldOfView / 2));
	}

	void PlaceCameraBasedOnCheating(out Vector3 position, out Quaternion rotation) {		
		position = this.arCamera.transform.position;
		rotation = this.arCamera.transform.rotation;
	}
}
