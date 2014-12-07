using UnityEngine;
using System.Linq;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

public class ThrowBall : MonoBehaviour {

	public GameObject[] trackers;
	public Vector2[] points;

	float speed = 256;
	float spawnDistance = 50;

	// Use this for initialization
	void Start () {
		
	}

	Matrix h;
	
	// Update is called once per frame
	void Update () {
		Vector2[] screenPoints = trackers.Select(obj => {
			Vector3 screen = Camera.main.WorldToScreenPoint(obj.transform.position);
			return new Vector2(screen.x, screen.y);
		}).ToArray();

		h = MathSupport.ComputeHomography(screenPoints, points);
		
		if (Input.GetMouseButtonDown(0)) {
			this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * spawnDistance;
			this.rigidbody.velocity = Camera.main.transform.forward * speed;
		}
	}

	void OnGUI() {
		if (h != null) {
			//GUI.Label(new Rect(0, 0, 100, 100), h.ToString());
		}
	}
}
