using UnityEngine;
using System.Collections;

public class Paddle : MonoBehaviour {

	public Camera followCamera;
	public float forwardDistance = 100;
	public float lerpSpeed = 0.05f;
	
	void Update () {
		Vector3 dest = this.followCamera.transform.position + this.followCamera.transform.forward * this.forwardDistance;
		this.transform.position = Vector3.Lerp(this.transform.position, dest, this.lerpSpeed);
		this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.followCamera.transform.rotation, this.lerpSpeed);
	}
}
