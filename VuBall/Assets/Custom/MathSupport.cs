using UnityEngine;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

public class MathSupport {
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

		Matrix x = a.QRDecomposition.Solve(b);
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
