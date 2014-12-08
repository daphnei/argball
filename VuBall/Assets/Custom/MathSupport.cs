using UnityEngine;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;

public static class MathSupport {

	public static Vector ToHomogeneousVector(this Vector2 vector) {
		Vector v = new Vector(3);
		v[0] = vector.x;
		v[1] = vector.y;
		v[2] = 1;
		return v;
	}	

	public static Vector3 ToVector3(this Vector vector) {
		if (vector.Length != 3) {
			throw new Exception("You suck.");
		}
		return new Vector3((float)vector[0], (float)vector[1], (float)vector[2]);
	}

	/// <summary>
	/// Code adapted from this article:
	/// http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
	/// </summary>
	public static UnityEngine.Quaternion ConvertRotationMatrixToQuaternion(Matrix matrix) {
		float m00 = (float)matrix[0,0];
		float m01 = (float)matrix[0,1];
		float m02 = (float)matrix[0,2];
		float m10 = (float)matrix[1,0];
		float m11 = (float)matrix[1,1];
		float m12 = (float)matrix[1,2];
		float m20 = (float)matrix[2,0];
		float m21 = (float)matrix[2,1];
		float m22 = (float)matrix[2,2];
		float qw, qx, qy, qz;
		float tr = m00 + m11 + m22;		
		if (tr > 0) { 
		  float S = (float)Math.Sqrt(tr + 1.0f) * 2f; // S=4*qw 
		  qw = 0.25f * S;
		  qx = (m21 - m12) / S;
		  qy = (m02 - m20) / S; 
		  qz = (m10 - m01) / S; 
		} else if ((m00 > m11)&(m00 > m22)) { 
		  float S = (float)Math.Sqrt(1.0f + m00 - m11 - m22) * 2; // S=4*qx 
		  qw = (m21 - m12) / S;
		  qx = 0.25f * S;
		  qy = (m01 + m10) / S; 
		  qz = (m02 + m20) / S; 
		} else if (m11 > m22) { 
		  float S = (float)Math.Sqrt(1.0f + m11 - m00 - m22) * 2; // S=4*qy
		  qw = (m02 - m20) / S;
		  qx = (m01 + m10) / S; 
		  qy = 0.25f * S;
		  qz = (m12 + m21) / S; 
		} else { 
		  float S = (float)Math.Sqrt(1.0f + m22 - m00 - m11) * 2; // S=4*qz
		  qw = (m10 - m01) / S;
		  qx = (m02 + m20) / S;
		  qy = (m12 + m21) / S;
		  qz = 0.25f * S;
		}

		return new UnityEngine.Quaternion(qx, qy, qz, qw);
	}

	public static Matrix ComputeHomography(Vector2[] first, Vector2[] second) {
		int n = first.Length;
		if (n != second.Length) {
			return null;
		}
		if (n < 4) {
			return null;
		}

		Matrix a = new Matrix(n * 2, 8);
		Matrix b = new Matrix(n * 2, 1);
		for (int i = 0; i < n; i++) {
			int i1 = i * 2;
			a[i1, 0] = first[i].x;
			a[i1, 1] = first[i].y;
			a[i1, 2] = 1;
			a[i1, 3] = 0;
			a[i1, 4] = 0;
			a[i1, 5] = 0;			
			a[i1, 6] = -(first[i].x * second[i].x);
			a[i1, 7] = -(first[i].y * second[i].x);

			int i2 = i * 2 + 1;
			a[i2, 0] = 0;
			a[i2, 1] = 0;
			a[i2, 2] = 0;
			a[i2, 3] = first[i].x;
			a[i2, 4] = first[i].y;
			a[i2, 5] = 1;
			a[i2, 6] = -(first[i].x * second[i].y);
			a[i2, 7] = -(first[i].y * second[i].y);

			b[i1, 0] = second[i].x;
			b[i2, 0] = second[i].y;
		}

		Matrix x = a.Solve(b);
		Matrix result = new Matrix(3, 3);
		result[0, 0] = x[0, 0];
		result[0, 1] = x[1, 0];
		result[0, 2] = x[2, 0];
		result[1, 0] = x[3, 0];
		result[1, 1] = x[4, 0];
		result[1, 2] = x[5, 0];
		result[2, 0] = x[6, 0];
		result[2, 1] = x[7, 0];
		result[2, 2] = 1;
		return result;
	}
}
