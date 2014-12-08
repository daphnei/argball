using UnityEngine;
using System.Collections;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

public class CustomCamera : MonoBehaviour {

	public GameObject[] trackers;
	public Vector2[] points;
	
	// Update is called once per frame
	void Update () {
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

		Matrix camRotation = pose.GetMatrix(0, 2, 0, 2);
		Vector camTranslation = pose.GetColumnVector(3);
		
		Matrix actualTranslation = -1 * Matrix.Transpose(camRotation) * camTranslation.ToColumnMatrix();
		this.transform.position = actualTranslation.GetColumnVector(0).ToVector3();
	}
}
