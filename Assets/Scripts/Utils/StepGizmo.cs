using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepGizmo : MonoBehaviour
{
	public Shape points = new Shape();
	public int steps = 4;
	public float strideLength = .7f;
	public float strideWidth = .2f;
	public Quaternion strideRotationOffset;
	public float curvatureDegrees = 0f;
	public bool draw = true;
	public enum Side { Right, Left }
	public Side startWith = Side.Right;
	public float pelvisHeight = .82f;
	public float headHeight = 1.5f;
	private void OnDrawGizmos() {
		if (!draw) return;
		Vector3 pointPos = transform.position;
		Quaternion pointRot = transform.rotation;
		Quaternion stepCurvature = Quaternion.Euler(0,curvatureDegrees/steps,0);
		for (int i = 0; i < steps; i++) {
			Gizmos.color = Color.yellow.MultiplyAlpha(.3f);
			var mirrored = (startWith == Side.Right ? i : i + 1) % 2 > 0;
			pointRot *= stepCurvature;
			var stepPos = pointPos + (pointRot * Vector3.right) * strideWidth / 2 * (mirrored ? -1 : 1);
			var stepRot = pointRot * (mirrored ? strideRotationOffset : Quaternion.Inverse(strideRotationOffset));

			for (int j = 0; j < points.length; j++) {
				Vector3 p1;
				if (j == 0) p1 = points[points.length - 1];
				else p1 = points[j - 1];
				var p2 = points[j];
				if(mirrored) {
					p1 = p1.Invert(DefaultUtils.Axis.X);
					p2 = p2.Invert(DefaultUtils.Axis.X);
				}
				Gizmos.DrawLine(stepPos + pointRot * p1, stepPos + pointRot * p2);
			}
			var prevPointPos = pointPos;
			pointPos += pointRot * Vector3.forward * strideLength;
			Gizmos.DrawLine(prevPointPos, pointPos);
			Gizmos.color = new Color(.3f, .3f, 1, .3f);
			Gizmos.DrawLine(prevPointPos + transform.up * pelvisHeight, pointPos + transform.up * pelvisHeight);
			Gizmos.DrawLine(prevPointPos + transform.up * (pelvisHeight-.1f), prevPointPos + transform.up * (pelvisHeight+.1f));
			Gizmos.color = new Color(.3f, 1, .3f, .3f);
			Gizmos.DrawLine(prevPointPos + transform.up * headHeight, pointPos + transform.up * headHeight);
		}
	}

	[Serializable]
	public class Shape {
		[ShowInInspector]
		public int length;
		//public List<Vector3> points;
		public Vector3 this[int i] => new Vector3(
			(i >= 0 ?
			i <= length / 2 ?
			curve1.Evaluate((float)i / (length / 2)) :
			-curve2.Evaluate(((float)i - length / 2) / (length / 2)) : 0f) * soleWidth,
			0, (i <= length / 2 ? 0 : soleLength * 2) + (i <= length / 2 ? 1 : -1) * (float)i / (length / 2) * soleLength);
		public AnimationCurve curve1;
		public AnimationCurve curve2;
		public float soleWidth;
		public float soleLength;
	}
}
