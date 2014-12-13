using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The paddle that follows the camera and the ball is meant to
/// be hit against.
/// </summary>
public class Paddle : MonoBehaviour {
	// The camera the paddle should follow.
	public Camera followCamera;

	// Keep the paddle this far back from the camera's current position.
	public float forwardDistance = 100;

	// Whether or not to attempt to smooth the paddle's movement.
	public bool smooth = false;

	// Average the paddle's rotation and position over the cameras
	// values for the past this many frames.
	public int smoothingFrames = 20;

	private Quaternion smoothedRotation;
	private Vector3 smoothedPosition;

	// Store a history of the past rotations and positions of the camera.
	private Queue<Quaternion> savedRotations;
	private Queue<Vector3> savedPositions;

	void Start() {
		this.savedRotations = new Queue<Quaternion>(smoothingFrames);
		this.savedPositions = new Queue<Vector3>(smoothingFrames);
	}
	
	/// <summary>
	/// Keeps track of the position of the camera over several frames and uses the
	/// average position for the paddle position.
	/// </summary>
	void Update () {
		// The destination is selected as the region in front of the camera.
		Vector3 destination = this.followCamera.transform.position + this.followCamera.transform.forward * this.forwardDistance;
		SmoothCamera smooth = this.followCamera.GetComponent<SmoothCamera>();
		Quaternion rotation = this.followCamera.transform.rotation;
		if (smooth != null) {
			destination = smooth.smoothedPosition;
			if (smooth)
				this.transform.rotation = smooth.smoothedRotation;
			destination += this.transform.forward * this.forwardDistance;
			rotation = smooth.smoothedRotation;
		}
		
		if (savedRotations.Count >= smoothingFrames) {
			savedRotations.Dequeue();
			savedPositions.Dequeue();
		}

		savedRotations.Enqueue(rotation);
		savedPositions.Enqueue(destination);

		Vector4 avgr = Vector4.zero;
		foreach (Quaternion singleRotation in savedRotations) {
			Math3d.AverageQuaternion(ref avgr, singleRotation, savedRotations.Peek(), savedRotations.Count);
		}

		Vector3 avgp = Vector3.zero;
		foreach (Vector3 singlePosition in savedPositions) {
			avgp += singlePosition;
		}
		avgp /= savedPositions.Count;

		smoothedRotation = new Quaternion(avgr.x, avgr.y, avgr.z, avgr.w);
		smoothedPosition = avgp;

		if (smooth) {
			this.transform.position = smoothedPosition;
			this.transform.rotation = smoothedRotation;
		} else {
			this.transform.position = this.followCamera.transform.position;
			this.transform.rotation = this.followCamera.transform.rotation;
		}
	}
}
