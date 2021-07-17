﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	public static class Utils {
		/// <summary>
		/// Destroys Game Object in Editor or in play
		/// </summary>
		public static void Destroy(UnityEngine.Object o) {
			try {
#if UNITY_EDITOR
				if (Application.isEditor)
					GameObject.DestroyImmediate(o);
				else
#endif
					GameObject.Destroy(o);
				return;
			} catch (Exception ex) {
				Debug.LogError($"Exception while deleting GameObject in Utils.Destroy: {ex.Message}");
			}
		}
		/// <summary>
		/// Destroys Game Objects in Editor or in play
		/// </summary>
		public static void Destroy(IEnumerable<Component> Component) {
			foreach (var comp in Component) {
				Destroy(comp.gameObject);
			}
		}
		/// <summary>
		/// Destroys Game Objects in Editor or in play
		/// </summary>
		public static void Destroy(IEnumerable<GameObject> GameObjects) {
			foreach (var go in GameObjects) {
				Destroy(go);
			}
		}


		public static T GetValue<T>(this SerializedProperty property) where T : class {
			object obj = property.serializedObject.targetObject;
			string path = property.propertyPath.Replace(".Array.data", "");
			string[] fieldStructure = path.Split('.');
			Regex rgx = new Regex(@"\[\d+\]");
			for (int i = 0; i < fieldStructure.Length; i++) {
				if (fieldStructure[i].Contains("[")) {
					int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
					obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
				} else {
					obj = GetFieldValue(fieldStructure[i], obj);
				}
			}
			return (T)obj;
		}

		public static bool SetValue<T>(this SerializedProperty property, T value) where T : class {
			object obj = property.serializedObject.targetObject;
			string path = property.propertyPath.Replace(".Array.data", "");
			string[] fieldStructure = path.Split('.');
			Regex rgx = new Regex(@"\[\d+\]");
			for (int i = 0; i < fieldStructure.Length - 1; i++) {
				if (fieldStructure[i].Contains("[")) {
					int index = System.Convert.ToInt32(new string(fieldStructure[i].Where(c => char.IsDigit(c)).ToArray()));
					obj = GetFieldValueWithIndex(rgx.Replace(fieldStructure[i], ""), obj, index);
				} else {
					obj = GetFieldValue(fieldStructure[i], obj);
				}
			}

			string fieldName = fieldStructure.Last();
			if (fieldName.Contains("[")) {
				int index = System.Convert.ToInt32(new string(fieldName.Where(c => char.IsDigit(c)).ToArray()));
				return SetFieldValueWithIndex(rgx.Replace(fieldName, ""), obj, index, value);
			} else {
				Debug.Log(value);
				return SetFieldValue(fieldName, obj, value);
			}
		}

		private static object GetFieldValue(string fieldName, object obj, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
			FieldInfo field = obj.GetType().GetField(fieldName, bindings);
			if (field != null) {
				return field.GetValue(obj);
			}
			return default(object);
		}

		private static object GetFieldValueWithIndex(string fieldName, object obj, int index, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
			FieldInfo field = obj.GetType().GetField(fieldName, bindings);
			if (field != null) {
				object list = field.GetValue(obj);
				if (list.GetType().IsArray) {
					return ((object[])list)[index];
				} else if (list is IEnumerable) {
					return ((IList)list)[index];
				}
			}
			return default(object);
		}

		public static bool SetFieldValue(string fieldName, object obj, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
			FieldInfo field = obj.GetType().GetField(fieldName, bindings);
			if (field != null) {
				field.SetValue(obj, value);
				return true;
			}
			return false;
		}

		public static bool SetFieldValueWithIndex(string fieldName, object obj, int index, object value, bool includeAllBases = false, BindingFlags bindings = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) {
			FieldInfo field = obj.GetType().GetField(fieldName, bindings);
			if (field != null) {
				object list = field.GetValue(obj);
				if (list.GetType().IsArray) {
					((object[])list)[index] = value;
					return true;
				} else if (value is IEnumerable) {
					((IList)list)[index] = value;
					return true;
				}
			}
			return false;
		}
	}
}