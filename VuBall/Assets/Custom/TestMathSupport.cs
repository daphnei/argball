using UnityEngine;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;

public static class TestMathSupport {

	public static void TestHomography()
	{
		Vector2[] first = { new Vector2(0.5f, 0.5f), new Vector2(2f, 14f), new Vector2(19f, 3f), new Vector2(19f, 23f) };
		Vector2[] second = {new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f)};

		Matrix result = MathSupport.ComputeHomography(first, second);
		for (int i = 0; i < 4; i++) {
			Matrix inputVector = first[i].ToHomogeneousVector().ToColumnMatrix();
			Matrix outputVector = second[i].ToHomogeneousVector().ToColumnMatrix();
			Matrix transformed = result * inputVector;
			transformed = transformed * (1/transformed[2, 0]);
			Debug.Log(transformed.ToString() + " == " + outputVector.ToString()); 
		}
	}
}
