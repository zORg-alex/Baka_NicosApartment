using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class StretchTo : MonoBehaviour {
    public Transform Target;
	void Update() {
        if (Target != null) {
			transform.rotation = Quaternion.LookRotation((Target.position - transform.position).normalized, -transform.forward) * Quaternion.AngleAxis(90,Vector3.right);
            transform.localScale = new Vector3(transform.localScale.x, transform.parent.InverseTransformPoint(Target.position).magnitude, transform.localScale.z);
		}
    }

#if UNITY_EDITOR
	private void OnDisable() {
		if (!Application.isPlaying)
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

	private void OnDrawGizmosSelected() {
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
