using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[ExecuteInEditMode]
public class GeorgeAnimationControllerScript : MonoBehaviour {
	public PlayerInput input;

	Animator anim;
	int Sit;
	int Walk;
	int Speed;
	int Turn;
	int TurnAngle;
	int SitMirror;

	public Vector3 destination;
	public NavMeshAgent agent;
	private Vector3 oldTarget;

	private void OnEnable() {
		input = new PlayerInput();
		input.Player.Accept.performed += Clicked;
		input.Player.Enable();
		agent = GetComponent<NavMeshAgent>();
		agent.updatePosition = false;
		anim = GetComponent<Animator>();

		Sit = Animator.StringToHash("Sit");
		Walk = Animator.StringToHash("Walk");
		Speed = Animator.StringToHash("Speed");
		Turn = Animator.StringToHash("Turn");
		TurnAngle = Animator.StringToHash("TurnAngle");
		SitMirror = Animator.StringToHash("SitMirror");
	}
	private void OnDisable() {
		input.Player.Accept.performed -= Clicked;
	}

	private void Clicked(InputAction.CallbackContext obj) {
		if (agent != null) {
			var ray = Camera.main.ScreenPointToRay(input.Player.ScreenPointer.ReadVector2());
			if (Physics.Raycast(ray, out var hit)) {
				if(NavMesh.SamplePosition( hit.point, out var navhit, 4, NavMesh.AllAreas))
					agent.destination = navhit.position;
			}
			if (oldTarget != agent.steeringTarget) {
				if (transform.position.DistanceTo(agent.destination) > .2f) {
					SetTurnAngle();
					anim.SetBool(Walk, true);
					anim.SetFloat(Speed, 1.5f);
					agentMoving = true;
				}
				oldTarget = agent.steeringTarget;
			}
		}
	}
	private IEnumerator TurnUpdate() {
		float ang;
		do {
			ang = SetTurnAngle();
			yield return null;
		} while (ang > .5f);
	}

	private float SetTurnAngle() {
		float ang = Quaternion.FromToRotation(transform.forward, (transform.InverseTransformPoint(agent.steeringTarget).normalized)).eulerAngles.y * Mathf.Deg2Rad;
		anim.SetFloat(TurnAngle, ang);
		return ang;
	}

	private void StopTurning() {
		StopAllCoroutines();
		StartCoroutine(TurnUpdate());
	}

	public bool agentMoving;
	public float zzz;
	public bool agentAtDestination => transform.position.DistanceTo(agent.steeringTarget) < .1f;
	private void Update() {
		zzz = transform.position.DistanceTo(agent.steeringTarget);
		if (agentMoving == agentAtDestination) {
			anim.SetBool(Walk, false);
			agentMoving = false;
		}

		//var m = input.Player.Move.ReadVector2();
		//if (m.magnitude > 0) {
		//	var ang = Quaternion.FromToRotation(transform.forward, (transform.InverseTransformPoint(agent.steeringTarget).normalized)).eulerAngles.y * Mathf.Deg2Rad;
		//	anim.SetFloat(TurnAngle, ang);
		//	anim.SetBool(Walk, true);
		//	anim.SetFloat(Speed, 1.5f);

		//	oldAngle = ang;
		//} else {
		//	anim.SetBool(Walk, false);
		//}
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.yellow.MultiplyAlpha(.3f);
		Gizmos.DrawLine(transform.position, agent.steeringTarget);
		Gizmos.color = Color.red.MultiplyAlpha(.3f);
		Gizmos.DrawSphere(agent.pathEndPosition, .2f);
	}
}
