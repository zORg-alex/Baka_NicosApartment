using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class StretchTo : MonoBehaviour {
    public Transform Target;
	public float Coef;
	public bool drawGizmo;

	void Update() {
        if (Target != null) {
			transform.rotation = Quaternion.LookRotation((Target.position - transform.position).normalized, -transform.forward) * Quaternion.AngleAxis(90,Vector3.right);
			var scale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
			transform.localScale = scale;
			scale.y = transform.InverseTransformPoint(Target.position).magnitude / Coef;
			transform.localScale = scale;
		}
    }

	[Button]
	void Initialize() {
		if (Target != null) {
			var scale = new Vector3(transform.localScale.x, 1, transform.localScale.z);
			Coef = transform.InverseTransformPoint(Target.position).magnitude;
		}
	}

#if UNITY_EDITOR
	private void OnDisable() {
		if (!Application.isPlaying && isActiveAndEnabled)
			StartCoroutine(RestoreTransform());
	}

	private IEnumerator RestoreTransform() {
		yield return null;
		SerializedObject serializedObject = new SerializedObject(transform);
		var rot = serializedObject.FindProperty("m_LocalRotation");
		var scl = serializedObject.FindProperty("m_LocalScale");
		rot.prefabOverride = false;
		scl.prefabOverride = false;
		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}
#endif

	private void OnDrawGizmos() {
		if (drawGizmo) {
			Gizmos.color = Color.red.MultiplyAlpha(.3f);
			Gizmos.DrawLine(transform.position, transform.position + transform.right * transform.lossyScale.x);
			Gizmos.color = Color.green.MultiplyAlpha(.3f);
			Gizmos.DrawLine(transform.position, transform.position + transform.up * transform.lossyScale.y);
			Gizmos.color = Color.blue.MultiplyAlpha(.3f);
			Gizmos.DrawLine(transform.position, transform.position + transform.forward * transform.lossyScale.z);
			Gizmos.color = Color.yellow.MultiplyAlpha(.3f);
			Gizmos.DrawSphere(Target.position, .01f);
		}
	}
}
