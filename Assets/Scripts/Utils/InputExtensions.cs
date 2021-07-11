using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputExtensions {
	public static bool ReadBool(this InputAction action) {
		var o = action.ReadValueAsObject();
		if (o is bool b) return b;
		else if (o is float f) return f > 0;
		return false;
	}
	public static float ReadFloat(this InputAction action) => action.ReadValue<float>();
	public static Vector2 ReadVector2(this InputAction action) => action.ReadValue<Vector2>();
	public static Vector3 ReadVector3(this InputAction action) => action.ReadValue<Vector3>();
}
