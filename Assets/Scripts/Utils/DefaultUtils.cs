using System;
using UnityEngine;

public static class DefaultUtils {

	public static float LerpTo(this float value, float targetValue, float t = .5f) =>
		Mathf.Lerp(value,targetValue,t);

	public static Vector3 LerpTo(this Vector3 value, Vector3 targetValue, float t = .5f) =>
		Vector3.Lerp(value, targetValue, t);
	public static Vector2 LerpTo(this Vector2 value, Vector2 targetValue, float t = .5f) =>
		Vector2.Lerp(value, targetValue, t);

	public static float Clamp(this float value, float minValue, float maxValue) =>
		Math.Min(Math.Max(value, minValue), maxValue);

	public static int Clamp(this int value, int minValue, int maxValue) =>
		Math.Min(Math.Max(value, minValue), maxValue);

	/// <summary>
	/// One minus value. Like 1 - (0, 1, 0) => (1, 0, 1).
	/// </summary>
	/// <param name="v"></param>
	/// <returns></returns>
	public static Vector3 OneMinus(this Vector3 v) => Vector3.one - v;

	public enum Direction { up, down, right, left, forward, back }
	/// <summary>
	/// returns vector with dir = 0 and multiplies by direction e.g. down is (-x, 0, -z)
	/// </summary>
	/// <param name="v"></param>
	/// <param name="dir"></param>
	/// <returns></returns>
	public static Vector3 Not(this Vector3 v, Direction dir) {
		switch (dir) {
			case Direction.up:
				return new Vector3(v.x, 0, v.z);
			case Direction.down:
				return new Vector3(-v.x, 0, -v.z);
			case Direction.right:
				return new Vector3(0, v.y, v.z);
			case Direction.left:
				return new Vector3(0, -v.y, -v.z);
			case Direction.forward:
				return new Vector3(v.x, v.y, 0);
			case Direction.back:
				return new Vector3(-v.x, -v.y, 0);
			default:
				return Vector3.one;
		}
	}
	public enum Axis { X, Y, Z }
	public static Vector3 Invert(this Vector3 v, Axis axis) => new Vector3((axis == Axis.X ? -1 : 1) * v.x, (axis == Axis.Y ? -1 : 1) * v.y, (axis == Axis.Z ? -1 : 1) * v.z);
	/// <summary>
	/// Returns new vector with y = 0
	/// </summary>
	public static Vector3 Horizontal(this Vector3 v) => new Vector3(v.x, 0, v.z);
	public static Vector3 SetY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);

	public static float DistanceTo(this Vector3 vector1, Vector3 vector2) =>
		Vector3.Distance(vector1, vector2);

	public static Color MultiplyAlpha(this Color c, float alpha) => new Color(c.r, c.g, c.b, c.a * alpha);

	public static float Abs(this float v) => Math.Abs(v);
	public static int Abs(this int v) => Math.Abs(v);
	public static float Deg2Rad(this float v) => v * Mathf.Deg2Rad;
	public static float Rad2Deg(this float v) => v * Mathf.Rad2Deg;

	public static Quaternion Inverted(this Quaternion q) => Quaternion.Inverse(q);
}