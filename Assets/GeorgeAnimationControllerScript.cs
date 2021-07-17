using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GeorgeAnimationControllerScript : Utils.AssemblyReloadableMonoBehaviour {
	public InputAction click;
	public InputAction mousePos;
	public InputAction WASD;

	public Vector3 destination;
	public bool isMoving;

	public GeorgeAnimationControllerScript() {
		Initialize = Init;
	}
	private void Init() {
		click = new InputAction("", InputActionType.Value, "<Mouse>/leftButton");
		click.Enable();
		click.performed += Clicked;
		mousePos = new InputAction("", InputActionType.Value, "<Mouse>/position");
		mousePos.Enable();
		WASD = new InputAction("WASD");
		InputActionSetupExtensions.AddCompositeBinding(WASD, "WASD")
		  .With("Up", "<Keyboard>/w", "Keyboard&Mouse")
		  .With("Down", "<Keyboard>/s", "Keyboard&Mouse")
		  .With("Left", "<Keyboard>/a", "Keyboard&Mouse")
		  .With("Right", "<Keyboard>/d", "Keyboard&Mouse");
		WASD.Enable();
	}

	private void Clicked(InputAction.CallbackContext obj) {
		throw new NotImplementedException();
	}
}
