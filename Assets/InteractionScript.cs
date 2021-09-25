using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Utility;
using static AnimationProperty;

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
}

[Serializable]
public class AnimationProperty {
	public string name;
	public enum ValueType { Float, Integer, Bool, Trigger }
	public ValueType valueType;
	public enum Time { EnterBegin = 0, EnterEnd = 1, ExitBegin = 2, ExitEnd = 3}
	[SerializeField]
	int[] intValues = new int[4];
	[SerializeField]
	float[] floatValues = new float[4];
	[SerializeField]
	bool[] boolValues = new bool[4];
	[SerializeField]
	bool[] TriggerValues = new bool[4];
	[SerializeField]
	bool[] hasValue = new bool[4];

	public void ConvertValues(ValueType newVal) {
		if (newVal == ValueType.Float && valueType == ValueType.Integer) {
			for (int i = 0; i < 4; i++) {
				floatValues[i] = intValues[i];
			}
		} else if (newVal == ValueType.Integer && valueType == ValueType.Float) {
			for (int i = 0; i < 4; i++) {
				intValues[i] = (int)floatValues[i];
			}
		} else if (newVal == ValueType.Bool) {
			for (int i = 0; i < 4; i++) {
				boolValues[i] = (valueType == ValueType.Integer) ? intValues[i] > 0 : valueType == ValueType.Float ? floatValues[i] > 0 : valueType == ValueType.Trigger? TriggerValues[i] : false;
			}
		} else if (newVal == ValueType.Trigger) {
			for (int i = 0; i < 4; i++) {
				TriggerValues[i] = (valueType == ValueType.Integer) ? intValues[i] > 0 : valueType == ValueType.Float ? floatValues[i] > 0 : valueType == ValueType.Bool ? boolValues[i] : false;
			}
		}
		valueType = newVal;
	}

	public object GetValue(Time time) =>
		valueType == ValueType.Float ? floatValues[(int)time] :
		valueType == ValueType.Integer ? intValues[(int)time] :
		valueType == ValueType.Bool ? boolValues[(int)time] :
		valueType == ValueType.Trigger ? TriggerValues[(int)time] : default(object);

	public object GetValueOnEnter(out Time time) {
		if (HasValue(Time.EnterBegin)) {
			time = Time.EnterBegin;
			return GetValue(time);
		} else {
			time = Time.EnterEnd;
			return GetValue(time);
		}
	}
	public bool HasValue(Time time) =>
		hasValue[(int)time];
	public bool HasEnterValue() =>
		hasValue[0] || hasValue[1];
	public bool HasExitValue() =>
		hasValue[2] || hasValue[3];

	public void SetValue(float value, Time time) {
		floatValues[(int)time] = value;
		hasValue[(int)time] = true;
	}
	public void SetValue(int value, Time time) {
		intValues[(int)time] = value;
		hasValue[(int)time] = true;
	}
	public void SetValue(bool value, Time time) {
		boolValues[(int)time] = value;
		hasValue[(int)time] = true;
	}
	public void SetTrigger(bool value, Time time) {
		TriggerValues[(int)time] = value;
		hasValue[(int)time] = value;
	}

	public void RemoveValue(Time time) { 
		hasValue[(int)time] = false;
		floatValues[(int)time] = default;
		intValues[(int)time] = default;
		boolValues[(int)time] = default;
		TriggerValues[(int)time] = default;
	}
	internal void SetAnimatorValue(Animator anim, Time time) {
		switch (valueType) {
			case ValueType.Float:
				anim.SetFloat(name, (float)GetValue(time));
				break;
			case ValueType.Integer:
				anim.SetInteger(name, (int)GetValue(time));
				break;
			case ValueType.Bool:
				anim.SetBool(name, (bool)GetValue(time));
				break;
			case ValueType.Trigger:
				if (GetValue(time) != null) anim.SetTrigger(name);
				break;
			default:
				break;
		}
	}

	internal void SetDefault(Time time) {
		if (valueType == ValueType.Float) SetValue(0, time);
		else if (valueType == ValueType.Integer) SetValue(0, time);
		else if (valueType == ValueType.Bool) SetValue(false, time);
		else if (valueType == ValueType.Trigger) SetTrigger(true, time);
	}
}

[CustomPropertyDrawer(typeof(AnimationProperty))]
public class InteractionAnimSetValueDrawer : PropertyDrawer {
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return 0;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!initialized) Initialize(property);
		GUILayout.BeginHorizontal();
		obj.name = GUI.TextField(GUILayoutUtility.GetRect(100,20),  obj.name);
		AnimationProperty.ValueType newVal = (AnimationProperty.ValueType)EditorGUILayout.EnumPopup(oldValueType);
		EditorGUILayout.EndHorizontal();
		if (newVal != oldValueType) {
			obj.ConvertValues(newVal);
			oldValueType = newVal;
		}
		DrawLineFor(AnimationProperty.Time.EnterBegin, "Enter Begin");
		DrawLineFor(AnimationProperty.Time.EnterEnd, "Enter Final");
		DrawLineFor(AnimationProperty.Time.ExitBegin, "Exit Begin");
		DrawLineFor(AnimationProperty.Time.ExitEnd, "Exit Final");
	}

	private void DrawLineFor(AnimationProperty.Time time, string name) {
		EditorGUILayout.BeginHorizontal();
		if (!obj.HasValue(time)) {
			EditorGUILayout.LabelField(name);
			if (GUILayout.Button("+", GUILayout.Width(22)))
				obj.SetDefault(time);
		} else {
			if (obj.valueType == AnimationProperty.ValueType.Bool)
				obj.SetValue(EditorGUILayout.Toggle(name, (bool)obj.GetValue(time)), time);
			else if (obj.valueType == AnimationProperty.ValueType.Float)
				obj.SetValue(EditorGUILayout.FloatField(name, (float)obj.GetValue(time)), time);
			else if (obj.valueType == AnimationProperty.ValueType.Integer)
				obj.SetValue(EditorGUILayout.IntField(name, (int)obj.GetValue(time)), time);
			if (GUILayout.Button("-", GUILayout.Width(22)))
				obj.RemoveValue(time);
		}
		EditorGUILayout.EndHorizontal();
	}

	private void Initialize(SerializedProperty property) {
		valueType = property.FindPropertyRelative("valueType");
		oldValueType = (AnimationProperty.ValueType)valueType.enumValueIndex;

		obj = property.GetValue<AnimationProperty>();
		initialized = true;
	}

	bool initialized;
	private SerializedProperty valueType;
	private AnimationProperty.ValueType oldValueType;
	private AnimationProperty obj;
}