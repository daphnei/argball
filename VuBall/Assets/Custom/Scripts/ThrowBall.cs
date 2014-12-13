using UnityEngine;
using System.Linq;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// When the user clicks on the screen, throw the ball out from the paddle.
/// A Unity rigidbody should be attached to this object to ensure it uses physics.
/// </summary>
public class ThrowBall : MonoBehaviour {

	public Camera throwCamera;

	public float speed = 256;
	public float spawnDistance = 50;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			this.transform.position = throwCamera.transform.position + throwCamera.transform.forward * spawnDistance;
			this.rigidbody.velocity = throwCamera.transform.forward * speed;
		}
	}
}
