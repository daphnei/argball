using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Paddle : MonoBehaviour {

	public Camera followCamera;
	public float forwardDistance = 100;

	public int smoothingFrames = 20;
	private Quaternion smoothedRotation;
	private Vector3 smoothedPosition;

	private Queue<Quaternion> savedRotations;
	private Queue<Vector3> savedPositions;

	void Start() {
		this.savedRotations = new Queue<Quaternion>(smoothingFrames);
		this.savedPositions = new Queue<Vector3>(smoothingFrames);
	}
	
	void Update () {
		Vector3 destination = this.followCamera.transform.position + this.followCamera.transform.forward * this.forwardDistance;
		SmoothCamera smooth = this.followCamera.GetComponent<SmoothCamera>();
		Quaternion rotation = this.followCamera.transform.rotation;
		if (smooth != null) {
			destination = smooth.smoothedPosition;
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
		this.transform.position = smoothedPosition;
		this.transform.rotation = smoothedRotation;
	}
}
