using UnityEngine;
using System.Linq;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

public class ThrowBall : MonoBehaviour {

	public Camera throwCamera;

	public float speed = 256;
	public float spawnDistance = 50;

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

	void OnCollisionEnter() {
		Debug.Log("SDF");
	}
}
