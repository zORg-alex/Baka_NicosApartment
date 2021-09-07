using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickControl : MonoBehaviour, IOnMouseClick
{
	private PlayerInput input;

	public Transform MouseOverObject { get; private set; }
	public Vector3 MouseOverPoint { get; private set; }
	public UnityEvent<ClickControl> OnClick;

	private void OnEnable() {
		input = new PlayerInput();
		input.Player.Accept.Enable();
		input.Player.Accept.performed += Click;
	}

	private void Click(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		if (enabled) {
			if (Physics.Raycast(Camera.main.ScreenPointToRay(input.Player.ScreenPointer.ReadVector2()), out var hit)) {
				if (hit.collider != null) {
					MouseOverObject = hit.transform;
					MouseOverPoint = hit.point;
				}
			}
		}
	}

	private void OnDisable() {
		input.Player.Accept.Disable();
		input.Player.Accept.performed -= Click;
	}
}

internal interface IOnMouseClick {
	Vector3 MouseOverPoint { get; }
	Transform MouseOverObject { get; }
}