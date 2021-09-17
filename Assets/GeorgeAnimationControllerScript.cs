using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Aud_Footsteps))]
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

	/// <summary>
	/// 90 deg in Animator turn animations
	/// </summary>
	public float turn90Deg = 1.5f;
	public float maxSlightTurn = .5f;

	private void OnEnable() {
		input = new PlayerInput();
		input.Player.Accept.performed += Clicked;
		input.Player.Enable();
		agent = GetComponent<NavMeshAgent>();
		agent.updatePosition = false;
		agent.updateRotation = false;
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
		}
	}
	private IEnumerator TurnUpdate() {
		float turn;
		SetTurnAngle();
		do {
			turn = GetTurnVal();
			yield return null;
		} while (Mathf.Abs(turn) > maxSlightTurn);

		Debug.Log($"Turn finished, GetTurnVal() : {GetTurnVal()}");
		SetTurnAngle();
	}

	private float SetTurnAngle(float? ang = null) {
		if (ang == null) ang = GetTurnVal();
		anim.SetFloat(TurnAngle, ang.Value);
		return ang.Value;
	}

	private float GetTurnVal() {
		var ang = Quaternion.FromToRotation(transform.forward, (transform.InverseTransformPoint(agent.steeringTarget).normalized)).eulerAngles.y;
		return (ang < 180 ? ang : -360 + ang) / (90 / turn90Deg);
	}

	private void RestartTurning() {
		if (currentTurnCoroutine != null)
			StopCoroutine(currentTurnCoroutine);
		currentTurnCoroutine = TurnUpdate();
		StartCoroutine(currentTurnCoroutine);
	}

	private IEnumerator currentTurnCoroutine;

	public bool agentMoving;
	public float zzz;
	public bool ignoreInput;

	public bool agentAtDestination => transform.position.DistanceTo(agent.steeringTarget) < agent.stoppingDistance;
	private void Update() {
		if (ignoreInput) return;
		if (agentAtDestination && agentMoving) {
			Debug.Log($"Target reached, stopping");
			anim.SetBool(Walk, false);
			SetTurnAngle(0);
			agentMoving = false;
		}

		if (oldTarget != agent.steeringTarget) {
			if (!agentAtDestination) {
				Debug.Log($"New target, start moving, GetTurnVal() : {GetTurnVal()}");
				RestartTurning();
				anim.SetBool(Walk, true);
				anim.SetFloat(Speed, 1.5f);
				agentMoving = true;
			}
			oldTarget = agent.steeringTarget;
		}

		if (agentMoving && currentTurnCoroutine == null && GetTurnVal() > maxSlightTurn) {
			Debug.Log($"agentMoving && notTurning && GetTurnVal() : {GetTurnVal()}");
			RestartTurning();
		} 

		agent.nextPosition = transform.position;
	}
	private List<Vector3> _gizmoPoints = new List<Vector3>();
	private List<(Vector3 p, Quaternion r)> _gizmoTransforms = new List<(Vector3 p, Quaternion r)>();

	[Button]
	private void GizmosClear() {
		_gizmoPoints.Clear();
		_gizmoTransforms.Clear();
	}

	private void OnDrawGizmos() {
		Gizmos.color = Color.yellow.MultiplyAlpha(.7f);
		Gizmos.DrawLine(transform.position, agent.steeringTarget);
		Gizmos.color = Color.blue.MultiplyAlpha(.7f);
		Gizmos.DrawLine(transform.position, transform.TransformPoint(transform.forward));
		Gizmos.color = new Color(1,1,.5f).MultiplyAlpha(.3f);
		Gizmos.DrawSphere(agent.pathEndPosition, .2f);

		Gizmos.color = Color.magenta.MultiplyAlpha(.3f);
		foreach (var p in _gizmoPoints) {
			Gizmos.DrawCube(p, Vector3.one * .05f);
		}
		foreach (var t in _gizmoTransforms) {
			Gizmos.color = Color.red.MultiplyAlpha(.3f);
			Gizmos.DrawLine(t.p, t.r * Vector3.right * .05f);
			Gizmos.color = Color.green.MultiplyAlpha(.3f);
			Gizmos.DrawLine(t.p, t.r * Vector3.up * .05f);
			Gizmos.color = Color.blue.MultiplyAlpha(.3f);
			Gizmos.DrawLine(t.p, t.r * Vector3.forward * .05f);
		}
	}

	/// <summary>
	/// Animations have this, just to clean up the console fro now
	/// </summary>
	public void StopTurning() { Debug.Log("Animation signalled for step stop"); }

	[Button]
	public void TestTurn() {
		zzz = GetTurnVal();
	}
}
