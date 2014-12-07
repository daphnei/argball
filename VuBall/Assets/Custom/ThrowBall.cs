using UnityEngine;
using System.Linq;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

public class ThrowBall : MonoBehaviour {

	public GameObject[] trackers;
	public Vector2[] points;
	public GameObject secondCamera;

	float speed = 256;
	float spawnDistance = 50;

	// Use this for initialization
	void Start () {
		TestMathSupport.TestHomography();
	}

	// Update is called once per frame
	void Update () {
		this.CalculateCameraParameters();
		
		if (Input.GetMouseButtonDown(0)) {
			this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * spawnDistance;
			this.rigidbody.velocity = Camera.main.transform.forward * speed;
		}
	}

	private void CalculateCameraParameters() {
		Vector2[] screenPoints = trackers.Select(obj => {
			Vector3 screen = Camera.main.WorldToScreenPoint(obj.transform.position);
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

		this.secondCamera.transform.position = pose.GetColumnVector(2).ConvertVectorToVector3();
	}

	void OnGUI() {
		/*if (homography != null) {
			GUI.Label(new Rect(0, 0, 100, 100), homography.ToString());
		}*/
	}
}
