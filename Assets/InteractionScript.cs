using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class InteractionScript : SerializedMonoBehaviour, IInteractable {
	[ShowInInspector,OdinSerialize]
	public Vector3 InteractionEntryPoint { get; private set; }
	public Vector3 InteractionEntryWorldPoint => transform.TransformPoint(InteractionEntryRotation * InteractionEntryPoint);

	[ShowInInspector, OdinSerialize]
	public Quaternion InteractionEntryRotation { get; private set; } = Quaternion.identity;
	public Quaternion InteractionEntryWorldRotation => transform.rotation * InteractionEntryRotation;

	[ShowInInspector, OdinSerialize]
	public Vector3 InteractionEndPoint { get; private set; }
	public Vector3 InteractionEndWorldPoint => transform.TransformPoint(InteractionEndRotation * InteractionEndPoint);

	[ShowInInspector, OdinSerialize]
	public Quaternion InteractionEndRotation { get; private set; } = Quaternion.identity;
	public Quaternion InteractionEndWorldRotation => transform.rotation * InteractionEndRotation;

#if UNITY_EDITOR
	public Mesh GizmoMeshEntry;
	public Mesh GizmoMeshEnd;
	public Vector3 GizmoScale = Vector3.one;
	private void Awake() {
		if (GizmoMeshEntry == null) {
			GizmoMeshEntry = new Mesh() {
				name = "Triangle",
				vertices = new Vector3[] { new Vector3(0, 0, .5f), new Vector3(-.3f, 0, -.3f), new Vector3(.3f, 0, -.3f) },
				triangles = new int[] { 0, 1, 2, 2, 1, 0 }
			};
			GizmoMeshEntry.RecalculateNormals();
		}
		if (GizmoMeshEnd == null) {
			GizmoMeshEnd = new Mesh() {
				name = "Triangle",
				vertices = new Vector3[] { new Vector3(0, 0, .5f), new Vector3(-.3f, 0, -.3f), new Vector3(.3f, 0, -.3f) },
				triangles = new int[] { 0, 1, 2, 2, 1, 0 }
			};
			GizmoMeshEnd.RecalculateNormals();
		}
	}

	private void OnDrawGizmosSelected() {
		if (Selection.activeGameObject != transform.gameObject) return;

		Gizmos.color = new Color(0, 1, 0, .3f);
		Gizmos.DrawMesh(GizmoMeshEntry, transform.TransformPoint(InteractionEntryPoint), InteractionEntryWorldRotation);
		Gizmos.color = new Color(1, 0, 0, .3f);
		Gizmos.DrawLine(InteractionEntryWorldPoint + (InteractionEntryWorldRotation * Vector3.right) * .1f, InteractionEntryWorldPoint);
		Gizmos.color = new Color(0, 1, 0, .3f);
		Gizmos.DrawLine(InteractionEntryWorldPoint + (InteractionEntryWorldRotation * Vector3.up) * .1f, InteractionEntryWorldPoint);
		Gizmos.color = new Color(0, 0, 1, .3f);
		Gizmos.DrawLine(InteractionEntryWorldPoint + (InteractionEntryWorldRotation * Vector3.forward) * .1f, InteractionEntryWorldPoint);

		Gizmos.color = new Color(1, 1, 0, .3f);
		Gizmos.DrawMesh(GizmoMeshEnd, InteractionEndWorldPoint, InteractionEndWorldRotation);
		Gizmos.color = new Color(1, 0, 0, .3f);
		Gizmos.DrawLine(InteractionEndWorldPoint + (InteractionEndWorldRotation * Vector3.right) * .1f, InteractionEndWorldPoint);
		Gizmos.color = new Color(0, 1, 0, .3f);
		Gizmos.DrawLine(InteractionEndWorldPoint + (InteractionEndWorldRotation * Vector3.up) * .1f, InteractionEndWorldPoint);
		Gizmos.color = new Color(0, 0, 1, .3f);
		Gizmos.DrawLine(InteractionEndWorldPoint + (InteractionEndWorldRotation * Vector3.forward) * .1f, InteractionEndWorldPoint);
	}
#endif
}



public interface IInteractable {
	Vector3 InteractionEntryPoint { get; }
	Quaternion InteractionEntryRotation { get; }
	Vector3 InteractionEndPoint { get; }
	Quaternion InteractionEndRotation { get; }
}