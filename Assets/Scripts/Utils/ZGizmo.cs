using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public static class ZGizmo
{
	public static void DrawWireCube(Vector3 position, Quaternion rotation, Vector3 size = default, Color color = default) {
		var oldColor = Gizmos.color;
		if (color != default) Gizmos.color = color;
		if (size == default) size = Vector3.one;
		var positions = new Vector3[] {
			rotation * new Vector3(size.x / 2, size.y / 2, size.z / 2) + position,
			rotation * new Vector3(size.x / 2, size.y / 2, size.z / -2) + position,
			rotation * new Vector3(size.x / 2, size.y / -2, size.z / -2) + position,
			rotation * new Vector3(size.x / 2, size.y / -2, size.z / 2) + position,
			rotation * new Vector3(size.x / -2, size.y / 2, size.z / 2) + position,
			rotation * new Vector3(size.x / -2, size.y / 2, size.z / -2) + position,
			rotation * new Vector3(size.x / -2, size.y / -2, size.z / -2) + position,
			rotation * new Vector3(size.x / -2, size.y / -2, size.z / 2) + position
		};

		int last = 3;
		for (int i = 0; i < 4; i++) {
			Gizmos.DrawLine(positions[last], positions[i]);
			Gizmos.DrawLine(positions[last + 4], positions[i + 4]);
			Gizmos.DrawLine(positions[i], positions[i + 4]);
			last = i;
		}

	}

	public static void DrawWireCircle(Vector3 position, Quaternion identity, float radius, int quality = 5, Color color = default) {
		var oldColor = Gizmos.color;
		if (color != default) {
			Gizmos.color = color;
		}

		var qualitySegment = 360f / (quality * 10f);
		var step = Quaternion.AngleAxis(qualitySegment, Vector3.up);
		var lastPoint = position + identity * Vector3.forward * radius;
		var rot = identity;
		for (int i = 1; i <= quality * 10; i++) {
			rot = rot * step;
			var point = position + rot * Vector3.forward * radius;
			Gizmos.DrawLine(lastPoint, point);
			lastPoint = point;
		}

		if (color != default) {
			Gizmos.color = oldColor;
		}
	}
	public static void DrawWireArc(Vector3 position, Quaternion identity, float radius, float angleStart, float angleEnd, int quality = 5, Color color = default) {
		var oldColor = Gizmos.color;
		if (color != default) {
			Gizmos.color = color;
		}

		var qualitySegment = 360f / (quality * 10f);
		var step = Quaternion.AngleAxis(qualitySegment, Vector3.up);
		var rot = identity * Quaternion.AngleAxis(angleStart, Vector3.up);
		var lastPoint = position + rot * Vector3.forward * radius;
		for (int i = 1; i <= quality * 10 / (360/(angleEnd - angleStart)); i++) {
			rot = rot * step;
			var point = position + rot * Vector3.forward * radius;
			Gizmos.DrawLine(lastPoint, point);
			lastPoint = point;
		}
		Gizmos.DrawLine(lastPoint, position + (identity * Quaternion.AngleAxis(angleEnd, Vector3.up) * Vector3.forward) * radius);

		if (color != default) {
			Gizmos.color = oldColor;
		}
	}
}
