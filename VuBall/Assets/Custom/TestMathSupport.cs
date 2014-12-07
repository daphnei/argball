using UnityEngine;
using System.Collections;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;

public static class TestMathSupport {

	public static void TestHomography()
	{
		Vector2[] pin = {new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f)};
		Vector2[] pout = {new Vector2(0.5f, 0.5f), new Vector2(12f, 14f), new Vector2(19f, 21f), new Vector2(19f, 2f)};

		Matrix result = MathSupport.ComputeHomography(pin, pout);
		Debug.Log(result);

		Matrix goal = new Matrix(3, 3);

		/*   -0.4846    0.4487    0.0072
  			 -0.5349    0.5247    0.0072
  			 -0.0395    0.0236    0.0144 */

		//And flipped
		/*    -0.3289    0.2802    0.0244
  			 -0.3304    0.2981    0.0161
  			 -0.3612    0.2802    0.6368
   */
	}
}
