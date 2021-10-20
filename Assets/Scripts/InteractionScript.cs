using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Utility;
using static AnimationProperty;

[SelectionBase]
[ExecuteInEditMode]
public class InteractionScript : SerializedMonoBehaviour, IInteractable {
	[ShowInInspector, OdinSerialize]
	public Vector3 InteractionEntryPointLocal { get; private set; }
	public Vector3 InteractionEntryPoint => transform.TransformPoint(InteractionEntryRotationLocal * InteractionEntryPointLocal);

	[ShowInInspector, OdinSerialize]
	public Quaternion InteractionEntryRotationLocal { get; private set; }
	public Quaternion InteractionEntryRotation => transform.rotation * InteractionEntryRotationLocal;

	[ShowInInspector, OdinSerialize]
	public Vector3 InteractionEndPointLocal { get; private set; }
	public Vector3 InteractionEndPoint => transform.TransformPoint(InteractionEndRotationLocal * InteractionEndPointLocal);

	[ShowInInspector, OdinSerialize]
	public Quaternion InteractionEndRotationLocal { get; private set; }
	public Quaternion InteractionEndRotation => transform.rotation * InteractionEndRotationLocal;

	[ShowInInspector]
	public AnimationProperty[] AnimationSetValues { get { return _animationSetValues; } private set { _animationSetValues = value; } }
	[SerializeField, HideInInspector]
	private AnimationProperty[] _animationSetValues = new AnimationProperty[0];

	public void ApplyFor(Animator anim, AnimationProperty.Time time) {
		foreach (var av in AnimationSetValues.Where(av=>av.HasValue(time))) {
			av.SetAnimatorValue(anim, time);
		}
	}

#if UNITY_EDITOR
	public Mesh GizmoMeshEntry;
	public Mesh GizmoMeshEnd;
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
		Gizmos.DrawMesh(GizmoMeshEntry, transform.TransformPoint(InteractionEntryPointLocal), InteractionEntryRotation);
		Gizmos.color = new Color(1, 0, 0, .3f);
		Gizmos.DrawLine(InteractionEntryPoint + (InteractionEntryRotation * Vector3.right) * .1f, InteractionEntryPoint);
		Gizmos.color = new Color(0, 1, 0, .3f);
		Gizmos.DrawLine(InteractionEntryPoint + (InteractionEntryRotation * Vector3.up) * .1f, InteractionEntryPoint);
		Gizmos.color = new Color(0, 0, 1, .3f);
		Gizmos.DrawLine(InteractionEntryPoint + (InteractionEntryRotation * Vector3.forward) * .1f, InteractionEntryPoint);

		Gizmos.color = new Color(1, 1, 0, .3f);
		Gizmos.DrawMesh(GizmoMeshEnd, InteractionEndPoint, InteractionEndRotation);
		Gizmos.color = new Color(1, 0, 0, .3f);
		Gizmos.DrawLine(InteractionEndPoint + (InteractionEndRotation * Vector3.right) * .1f, InteractionEndPoint);
		Gizmos.color = new Color(0, 1, 0, .3f);
		Gizmos.DrawLine(InteractionEndPoint + (InteractionEndRotation * Vector3.up) * .1f, InteractionEndPoint);
		Gizmos.color = new Color(0, 0, 1, .3f);
		Gizmos.DrawLine(InteractionEndPoint + (InteractionEndRotation * Vector3.forward) * .1f, InteractionEndPoint);
	}
#endif
}



public interface IInteractable {
	Vector3 InteractionEntryPointLocal { get; }
	Quaternion InteractionEntryRotationLocal { get; }
	Vector3 InteractionEndPointLocal { get; }
	Quaternion InteractionEndRotationLocal { get; }
	Vector3 InteractionEntryPoint { get; }
	Quaternion InteractionEntryRotation { get; }
	Vector3 InteractionEndPoint { get; }
	Quaternion InteractionEndRotation { get; }
	AnimationProperty[] AnimationSetValues {  get; }

	void ApplyFor(Animator anim, AnimationProperty.Time time);
}
