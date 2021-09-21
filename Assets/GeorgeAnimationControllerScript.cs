using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// Character movement controller <para/>
/// <see cref="Update"/>:
/// <list type="bullet">Stop after reaching destination (may be unnecessary)</list>
/// <list type="bullet">Restart turning after reaching a steering target</list>
/// <list type="bullet">Restart turning if off course</list>
/// <list type="bullet">Slow down if nearing a sharp turn || destination</list>
/// <see cref="StartLerpSpeedTo(float, float)"/> starts coroutine for speed change <para/>
/// <see cref="RestartTurning"/> start [again] turning coroutine when need to change angle
/// </summary>
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
	public float SharpTurn = 1.5f;
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
				if (NavMesh.SamplePosition(hit.point, out var navhit, 4, NavMesh.AllAreas))
					MoveTo(navhit.position);
			}
		}
	}

	/// <summary>
	/// Character callback for action to move to a destination
	/// </summary>
	public void MoveTo(Vector3 destination) {
		agent.destination = destination;
	}

	/// <summary>
	/// Turn Coroutine for alligning to a steering target
	/// </summary>
	/// <returns></returns>
	private IEnumerator TurnUpdate() {
		float turn = GetTurnVal();
		float oldTurn;
		SetTurnAngle();
		do {
			//Ugly, but Oh well... Crutches
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(agent.steeringTarget - transform.position), Time.deltaTime);
			yield return null;
			oldTurn = turn;
			turn = GetTurnVal();
			if (turn.Abs() > oldTurn.Abs() && turn.Abs() > SharpTurn) {
				SetTurnAngle();
			}
		} while (turn.Abs() > maxSlightTurn);

		Debug.Log($"Turn finished, GetTurnVal() : {GetTurnVal()}");
		SetTurnAngle();
		_gizmoTransforms.Add((transform.position, transform.rotation));
		currentTurnCoroutine = null;
	}

	private float SetTurnAngle() => SetTurnAngle(GetTurnVal());
	private float SetTurnAngle(float ang) {
		anim.SetFloat(TurnAngle, ang);
		return ang;
	}

	private float GetTurnVal() {
		var direction = transform.InverseTransformPoint(agent.steeringTarget.Horizontal()).normalized;
		return ConvertDegAngleToAnimAngle(ConvertDirectionToDegAngle(direction));
	}

	private float ConvertDirectionToDegAngle(Vector3 direction) {
		return Quaternion.FromToRotation(Vector3.forward, (direction)).eulerAngles.y;
	}

	private float ConvertDegAngleToAnimAngle(float ang) {
		return (ang < 180 ? ang : -360 + ang) / (90 / turn90Deg);
	}

	/// <summary>
	/// Restarts <see cref="TurnUpdate"/> Coroutine to allign to a steering target
	/// </summary>
	private void RestartTurning() {
		_gizmoTransforms.Add((transform.position, transform.rotation));
		if (currentTurnCoroutine != null)
			StopCoroutine(currentTurnCoroutine);
		currentTurnCoroutine = TurnUpdate();
		StartCoroutine(currentTurnCoroutine);
	}

	private IEnumerator currentTurnCoroutine;
	/// <summary>
	/// Called on last frame in animation clip for turning on spot
	/// So when we start turning on spot, we can start moving after full turn motion
	/// </summary>
	public void StopTurning() {
		Debug.Log("Animation signalled for step stop");
		SetTurnAngle();
		if (agentIsMoving)
			StartLerpSpeedTo(WalkSpeed, .2f);
	}
	/// <summary>
	/// Starts <see cref="LerpSpeedTo(float, float)"/> Coroutine and stops previous
	/// </summary>
	/// <param name="speed"></param>
	/// <param name="seconds"></param>
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

	/// <summary>
	/// Set in <see cref="Update"/>
	/// </summary>
	public bool agentIsMoving;
	/// <summary>
	/// Disables <see cref="Update"/>
	/// </summary>
	public bool ignoreInput;

	public float WalkSpeed = 1.5f;
	public float SharpTurnSlowDownDistance = .5f;
	public float SlowWalkSpeed = .7f;

	public bool NextCornerIsSharp;
	public bool nextCornerIsSharp() => agent.path.corners.Length > 1 ? ConvertDegAngleToAnimAngle(ConvertDirectionToDegAngle(transform.InverseTransformPoint(agent.path.corners[1] - agent.path.corners[0]).normalized)) > SharpTurn : false;

	/// <summary>How close to a sharp corner if there are > 1</summary>
	/// <returns>[0..1]</returns>
	public float ProximityToSharpCornerOrEnd() {
		var lastTargetOrSharpTurn = NextCornerIsSharp;
		if (agent.steeringTarget == agent.destination) lastTargetOrSharpTurn = true;

		return lastTargetOrSharpTurn ? 1 - transform.position.DistanceTo(agent.steeringTarget) / SharpTurnSlowDownDistance : 0;
	}

	Vector3 lastSteeringTarget;
	public bool agentAtDestination => transform.position.DistanceTo(agent.destination) < agent.stoppingDistance;

	private void Update() {
		if (agent.steeringTarget != lastSteeringTarget) {
			//Don't want to update every frame, since it's not going to change drastically
			NextCornerIsSharp = nextCornerIsSharp();
		}

		//Allign character to a terrain height; Raycast from hips to ground
		Physics.Raycast(transform.position + transform.up * .5f, Vector3.down, out var hit, LayerMask.GetMask("NavigationTargets"));
		transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);


		if (ignoreInput) return;
		if (agentAtDestination && agentIsMoving) {
			_gizmoTransforms.Add((transform.position, transform.rotation));
			Debug.Log($"Target reached, stopping");
			anim.SetBool(Walk, false);
			StartLerpSpeedTo(0f, .1f);
			SetTurnAngle(0);
			agentIsMoving = false;
		}

		if (oldTarget != agent.steeringTarget) {
			_gizmoPoints.Add(agent.steeringTarget);
			if (!agentAtDestination) {
				Debug.Log($"New target, start moving, GetTurnVal() : {GetTurnVal()}");
				RestartTurning();
				anim.SetBool(Walk, true);
				agentIsMoving = true;
			}
			oldTarget = agent.steeringTarget;
		}

		if (agentIsMoving && currentTurnCoroutine == null && GetTurnVal().Abs() > maxSlightTurn) {
			Debug.Log($"agentMoving && notTurning && GetTurnVal() : {GetTurnVal()}");
			RestartTurning();
		}

		if (NextCornerIsSharp || agent.steeringTarget == agent.destination) {
			var slowDownSpeed = agent.steeringTarget == agent.destination ? 0 : SlowWalkSpeed;
			if (transform.position.DistanceTo(agent.steeringTarget) < SharpTurnSlowDownDistance)
				anim.SetFloat(Speed, anim.GetFloat(Speed).LerpTo(slowDownSpeed, ProximityToSharpCornerOrEnd()));
		}

		agent.nextPosition = transform.position;
		lastSteeringTarget = agent.steeringTarget;
	}

	/* 
	 * ========================================================================================================
	 * Gizmo stuff
	 * ========================================================================================================
	 */

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
		Gizmos.DrawLine(transform.position, transform.position + (transform.forward * .3f));
		Gizmos.color = new Color(1,1,.5f).MultiplyAlpha(.3f);
		Gizmos.DrawSphere(agent.pathEndPosition, .2f);
		if (currentTurnCoroutine != null) {
			Gizmos.color = Color.green.MultiplyAlpha(.3f);
			Gizmos.DrawWireSphere(transform.position, .3f);
			Gizmos.DrawLine(transform.position, transform.position + Quaternion.LookRotation(transform.InverseTransformPoint(agent.steeringTarget).normalized, transform.up) * transform.forward);
		}
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
}
