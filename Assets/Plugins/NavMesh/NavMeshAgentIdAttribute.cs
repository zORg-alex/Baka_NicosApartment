using System;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class NavMeshAgentIdAttribute : PropertyAttribute {
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NavMeshAgentIdAttribute))]
public class NavMeshAgentIdDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		UnityEditor.AI.NavMeshComponentsGUIUtility.AgentTypePopup("Agent Type", property);
	}
}
#endif