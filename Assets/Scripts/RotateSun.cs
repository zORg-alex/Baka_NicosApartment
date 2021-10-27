using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteInEditMode]
public class RotateSun : MonoBehaviour
{
	public float time = 5f;
	float timeStarted;
	public float Angle = 120f;
	public Vector3 Axis = Vector3.right;
	Quaternion rotationAtStart;
	Quaternion rotationAtEnd;
    public InputAction action;
	public Light Light;
	public AnimationCurve lightIntensityOverDirection;
	public float lightMaxIntensity;
	bool rotateAgain;
	bool isRotating;


	private void OnEnable() {
        action.Enable();
		action.performed += Action_performed;
		Axis = Axis.normalized;
	}
	private void OnDisable() {
		action.Disable();
	}

	[Button("Rotate")]
	private void Action_performed(InputAction.CallbackContext e) {
		if (isRotating) {
			rotateAgain = true;
			return;
		}
		timeStarted = Time.time;
		rotationAtStart = transform.rotation;
		rotationAtEnd = transform.rotation * Quaternion.AngleAxis(Angle, Axis);
		StopAllCoroutines();
		StartCoroutine(Rotate());
	}

	IEnumerator Rotate() {
		isRotating = true;
		while (transform.rotation != rotationAtEnd) {
			yield return null;
			transform.rotation = Quaternion.Lerp(rotationAtStart, rotationAtEnd, (Time.time - timeStarted) / time);
			var dot = Vector3.Dot(Light.transform.forward, Vector3.down);
			Light.intensity = lightIntensityOverDirection.Evaluate(dot) * lightMaxIntensity;
		}
		isRotating = false;
		if (rotateAgain) {
			rotateAgain = false;
			Action_performed(default);
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.DrawLine(transform.position, transform.position + transform.TransformDirection(Axis));
	}
}
