using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class CopyRotation : MonoBehaviour {
    public Transform Target;
	public Axes InfluencedAxes;
	[Range(0f, 1f)]
	public float Influence;
	[Space(30)]
	public Quaternion transformInitialRotation;
	public Quaternion targetInitialRotation;
    // Update is called once per frame
    void Update() {
		if (Target != null) {
			var targetRot = Quaternion.Lerp(transformInitialRotation, Target.localRotation * Quaternion.Inverse(targetInitialRotation), Influence).eulerAngles;
			transform.localEulerAngles = new Vector3(
				(InfluencedAxes & Axes.X) == Axes.X ? targetRot.x : transform.localEulerAngles.x,
				(InfluencedAxes & Axes.Y) == Axes.Y ? targetRot.y : transform.localEulerAngles.y,
				(InfluencedAxes & Axes.Z) == Axes.Z ? targetRot.z : transform.localEulerAngles.z);
		}
	}

	[Button]
	private void SetInitial() {
		transformInitialRotation = transform.localRotation;
		targetInitialRotation = Target.localRotation;
	}

#if UNITY_EDITOR
	public bool showGizmo;
	private void OnDrawGizmos() {
		if (showGizmo) {
			Gizmos.color = Color.red.MultiplyAlpha(.3f);
			Gizmos.DrawLine(transform.position, transform.position + transform.right * transform.lossyScale.x);
			Gizmos.color = Color.green.MultiplyAlpha(.3f);
			Gizmos.DrawLine(transform.position, transform.position + transform.up * transform.lossyScale.y);
			Gizmos.color = Color.blue.MultiplyAlpha(.3f);
			Gizmos.DrawLine(transform.position, transform.position + transform.forward * transform.lossyScale.z);
		}
	}

	private void OnDisable() {
		if(!Application.isPlaying)
			StartCoroutine(RestoreTransform());
	}

	private IEnumerator RestoreTransform() {
		yield return null;
		SerializedObject serializedObject = new SerializedObject(transform);
		var rot = serializedObject.FindProperty("m_LocalRotation");
		rot.prefabOverride = false;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}
#endif
}
