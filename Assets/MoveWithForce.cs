using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveWithForce : MonoBehaviour {
	public InputAction screenPosition;
	public InputAction activation;

	public float vel = 100f;
	public Vector3 lastContact;
	private bool interaction;
	//public float forceMag;

	private void OnEnable() {
		screenPosition = new InputAction("", InputActionType.Value, "<Mouse>/position");
		activation = new InputAction("", InputActionType.Button, "<Mouse>/rightButton");
		activation.canceled += ctx => interaction = false;
		activation.performed += ctx => interaction = true;
		screenPosition.Enable();
		activation.Enable();
	}

	private void Update() {
		if (interaction) {
			var pos = screenPosition.ReadVector2();
			if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out var hitInfo, 10000)) {
				if (lastContact != default && hitInfo.point != default) {
					var delta = hitInfo.point - lastContact;
					var distToAnchor = hitInfo.articulationBody.anchorPosition.DistanceTo(hitInfo.point);
					var force = delta * vel / Time.fixedDeltaTime * hitInfo.articulationBody.mass / distToAnchor / distToAnchor / distToAnchor;
					//forceMag = force.magnitude;
					hitInfo.articulationBody.AddForceAtPosition(force, hitInfo.point);
				}
				lastContact = hitInfo.point;
			}
		} else if (lastContact != default)
			lastContact = default;
	}
}
