using UnityEngine;
using System.Linq;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

public class ThrowBall : MonoBehaviour {

	public Camera throwCamera;

	float speed = 256;
	float spawnDistance = 50;

	// Use this for initialization
	void Start () {
		TestMathSupport.TestHomography();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			this.transform.position = throwCamera.transform.position + throwCamera.transform.forward * spawnDistance;
			this.rigidbody.velocity = throwCamera.transform.forward * speed;
		}
	}
}
