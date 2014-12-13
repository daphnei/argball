using UnityEngine;
using System.Collections;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

/// <summary>
/// The camera that we implemented. If our math was correct, this
/// camera should have have the same position and rotation as the
/// Vuforia camera.
/// </summary>
public class CustomCamera : MonoBehaviour {

	public static bool CustomPlacement = true;
	public static bool CustomDebug = true;

	public Camera arCamera;
	public GameObject[] trackers;
	public Vector2[] points;

	public Matrix homography = null;
	public ReferenceCamera reference;

	// Update is called once per frame
	void Update () {
		this.camera.projectionMatrix = this.arCamera.projectionMatrix;

		Vector3 newPosition;
		Quaternion newRotation;
		if (CustomCamera.CustomPlacement) {
			this.PlaceCameraBasedOnHomographyOld(out newPosition, out newRotation);
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

		this.homography = MathSupport.ComputeHomography(screenPoints, points);
		
		Matrix complete = this.reference.GetTransformFromWorldToOther(homography).ToGenericMatrix().GetMatrix(0, 3, 0, 3);
		Matrix intrinsics = this.reference.GetCameraIntrinsicsMatrix().ToGenericMatrix().GetMatrix(0, 3, 0, 3);
		Matrix pose = intrinsics.Inverse() * complete;
		pose = pose.GetMatrix(0, 2, 0, 3);

		Matrix camRotation = pose.GetMatrix(0, 2, 0, 2).Inverse();
		Matrix camTranslation = -1 * camRotation.Inverse() * pose.GetColumnVector(3).ToColumnMatrix();

		position = this.transform.parent.position + camTranslation.GetColumnVector(0).ToVector3();
		rotation = this.transform.parent.rotation * MathSupport.ConvertRotationMatrixToQuaternion(camRotation);
	}

	/// <summary>
	/// An alternative camera placing algorithm that does not work well.
	/// </summary>
	void PlaceCameraBasedOnHomographyFull(out Vector3 position, out Quaternion rotation) {
		Vector2[] screenPoints = trackers.Select(obj => {
			Vector3 screen = this.arCamera.WorldToScreenPoint(obj.transform.position);
			return new Vector2(screen.x, screen.y);
		}).ToArray();
		
		homography = MathSupport.ComputeHomography(screenPoints, points);
		
		Matrix complete = this.reference.GetTransformFromWorldToOther(homography).ToGenericMatrix().GetMatrix (0, 3, 0, 3);
		Matrix intrinsics = this.reference.GetCameraIntrinsicsMatrix().ToGenericMatrix().GetMatrix(0, 3, 0, 3);
		
		Matrix pose = intrinsics.Inverse() * complete;
		
		// Matrix camRotation = pose.GetMatrix(0, 2, 0, 2).Inverse();
		// Matrix camTranslation = -1 * camRotation.Inverse() * pose.GetColumnVector(3).ToColumnMatrix();
		
		// position = this.transform.parent.position + camTranslation.GetColumnVector(0).ToVector3();
		rotation = Quaternion.identity; // this.transform.parent.rotation * MathSupport.ConvertRotationMatrixToQuaternion(camRotation);
		position = new Vector3(100, 100, 100);

		// Vector4 s = pose.ToMatrix4x4().inverse * new Vector4(0,0,0,1);
		// position = this.transform.parent.position + new Vector3 (s.x, s.y, s.z) / s.w;
		this.transform.camera.worldToCameraMatrix = pose.ToMatrix4x4();
	}

	/// <summary>
	/// Places this camera based on new homography code with pose building.
	/// </summary>
	void PlaceCameraBasedOnHomographyWithPoseBuilding(out Vector3 position, out Quaternion rotation) {
		Vector2[] screenPoints = trackers.Select(obj => {
			Vector3 screen = this.arCamera.WorldToScreenPoint(obj.transform.position);
			return new Vector2(screen.x, screen.y);
		}).ToArray();

		this.homography = MathSupport.ComputeHomography(screenPoints, points);

		Matrix complete = this.reference.GetTransformFromWorldToOther(homography).ToGenericMatrix().GetMatrix(0, 2, 0, 2);
		Matrix intrinsics = this.reference.GetCameraIntrinsicsMatrix().ToGenericMatrix().GetMatrix(0, 2, 0, 2);
		Matrix inverseComplete = intrinsics.Inverse() * complete;
		Matrix pose = Matrix.Identity(3, 4);

		// Set the first two columns of the pose.
		pose.SetColumnVector(inverseComplete.GetColumnVector(0).Normalize(), 0);
		pose.SetColumnVector(inverseComplete.GetColumnVector(1).Normalize(), 1);

		// Set the third column as the cross of the first two.
		Vector cross = pose.GetColumnVector(0).CrossMultiply(pose.GetColumnVector(1)).Normalize();
		pose.SetColumnVector(cross, 2);

		// Set the forth column of the pose matrix.
		float norm1 = (float)inverseComplete.GetColumnVector(0).Norm();
		float norm2 = (float)inverseComplete.GetColumnVector(1).Norm();
		float tnorm = (norm1 + norm2) / 2;
		pose.SetColumnVector(inverseComplete.GetColumnVector(2) / tnorm, 3);

		Matrix camRotation = pose.GetMatrix(0, 2, 0, 2).Inverse();
		Matrix camTranslation = -1 * camRotation.Inverse() * pose.GetColumnVector(3).ToColumnMatrix();

		position = this.transform.parent.position + camTranslation.GetColumnVector(0).ToVector3();
		rotation = this.transform.parent.rotation * MathSupport.ConvertRotationMatrixToQuaternion(camRotation);
	}

	/// <summary>
	/// A much older camera placement algorithm that tried to use a fixed reference plane
	/// and camera intrinsics sampled from Matlab.
	/// </summary>
	void PlaceCameraBasedOnHomographyOld(out Vector3 position, out Quaternion rotation) {
		Vector2[] screenPoints = trackers.Select(obj => {
			Vector3 screen = this.arCamera.WorldToScreenPoint(obj.transform.position);
			return new Vector2(screen.x, screen.y);
		}).ToArray();

		// Create an intrinsics matrix.
		Matrix intrinsics = getNexus5CameraIntrinsicsMatrix();

		homography = MathSupport.ComputeHomography(screenPoints, points);
		homography = intrinsics.Inverse() * homography;

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

		Matrix extrinsics = pose;

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

	/// <summary>
	/// Returns the hardcoded calibration for my Nexus 5 phone.
	/// </summary>
	/// <returns>A camera calibration matrix.</returns>
	Matrix getNexus5CameraIntrinsicsMatrix() {

		//Values in pixels, but these make the camera even worse.
		//Is there a unit mismatch? Should these be values in 
		//Unity units? Commenting this out for now.
		/*
		Matrix intrinsics = new Matrix(3, 3);
		intrinsics[0, 0] = 3235.21f;
		intrinsics[1, 1] = 3065.28f;
		intrinsics[0, 2] = 1578.19f;
		intrinsics[1, 2] = 1790.96f;
		intrinsics[2, 2] = 1;
		 */

		Matrix intrinsics = Matrix.Identity(3, 3);
		return intrinsics;
	}
}
