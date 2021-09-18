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
[SelectionBase]
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
		} while (turn.Abs() > maxSlightTurn);

		Debug.Log($"Turn finished, GetTurnVal() : {GetTurnVal()}");
		SetTurnAngle();
		_gizmoTransforms.Add((transform.position, transform.rotation));
		currentTurnCoroutine = null;
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
		_gizmoTransforms.Add((transform.position, transform.rotation));
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
			_gizmoTransforms.Add((transform.position, transform.rotation));
			Debug.Log($"Target reached, stopping");
			anim.SetBool(Walk, false);
			StartLerpSpeedTo(0f, .1f);
			SetTurnAngle(0);
			agentMoving = false;
		}

		if (oldTarget != agent.steeringTarget) {
			_gizmoPoints.Add(agent.steeringTarget);
			if (!agentAtDestination) {
				Debug.Log($"New target, start moving, GetTurnVal() : {GetTurnVal()}");
				RestartTurning();
				anim.SetBool(Walk, true);
				agentMoving = true;
			}
			oldTarget = agent.steeringTarget;
		}

		if (agentMoving && currentTurnCoroutine == null && GetTurnVal().Abs() > maxSlightTurn) {
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
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * .3f);
		Gizmos.color = new Color(1,1,.5f).MultiplyAlpha(.3f);
		Gizmos.DrawSphere(agent.pathEndPosition, .2f);

		Gizmos.color = Color.magenta;
		foreach (var p in _gizmoPoints) {
			Gizmos.DrawWireCube(p, Vector3.one * .05f);
		}
		foreach (var t in _gizmoTransforms) {
			Gizmos.color = Color.red;
			Gizmos.DrawLine(t.p, t.p + t.r * Vector3.right * .05f);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(t.p, t.p + t.r * Vector3.up * .05f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(t.p, t.p + t.r * Vector3.forward * .05f);
		}
	}

	/// <summary>
	/// Animations have this, just to clean up the console fro now
	/// </summary>
	public void StopTurning() {
		Debug.Log("Animation signalled for step stop");
		if (agentMoving)
			StartLerpSpeedTo(1.5f, .2f);
	}

	[Button]
	public void TestTurn() {
		zzz = GetTurnVal();
	}

	private void StartLerpSpeedTo(float speed, float seconds) {
		if (currentLerpSpeedTo != null)
			StopCoroutine(currentLerpSpeedTo);
		currentLerpSpeedTo = LerpSpeedTo(speed, seconds);
		StartCoroutine(currentLerpSpeedTo);
	}
	private IEnumerator currentLerpSpeedTo;
	private IEnumerator LerpSpeedTo(float speed, float seconds) {
		float startSpeed = anim.GetFloat(Speed);
		if (startSpeed != speed) {
			Debug.Log($"LerpSpeedTo started Time.time:{Time.time}");
			float startTime = Time.time;
			while (startTime + seconds > Time.time) {
				//Lerp from 0 to speed in given seconds timespan
				anim.SetFloat(Speed, startSpeed.LerpTo(speed, (Time.time - startTime) / seconds));
				yield return null;
			}
			anim.SetFloat(Speed, speed);
			currentLerpSpeedTo = null;
			Debug.Log($"LerpSpeedTo finished Time.time:{Time.time}");
		} else {
			Debug.Log($"LerpSpeedTo cancelled, speed is already {startSpeed}");
		}
	}
}
