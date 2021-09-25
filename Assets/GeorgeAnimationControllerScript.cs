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

	private IInteractable currentInteractableTarget;
	private void Clicked(InputAction.CallbackContext obj) {
		if (agent != null) {
			var ray = Camera.main.ScreenPointToRay(input.Player.ScreenPointer.ReadVector2());
			if (Physics.Raycast(ray, out var hit)) {
				if (hit.collider.GetComponent<IInteractable>() is IInteractable interactable) {
					MoveTo(interactable.InteractionEntryPoint);
					currentInteractableTarget = interactable;
				} else if (NavMesh.SamplePosition(hit.point, out var navhit, 4, NavMesh.AllAreas)) {
					MoveTo(navhit.position);
					//anim.SetBool(currentInteractableTarget.AnimationSetBools, false);
					if (currentInteractableTarget != null)
						foreach (var v in currentInteractableTarget.AnimationSetValues.Where(i => i.HasValue(AnimationProperty.Time.ExitBegin))) {
							v.SetAnimatorValue(anim, AnimationProperty.Time.ExitBegin);
						}
					currentInteractableTarget = null;
				}
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
	private IEnumerator TurnUpdate(Quaternion? preciseRotation = null) {
		float turn;
		if (preciseRotation != null) {
			turn = ConvertDegAngleToAnimAngle((preciseRotation.Value * Quaternion.Inverse(transform.rotation)).eulerAngles.y);
		} else {
			turn = GetTurnVal();
		}
		float oldTurn;
		SetTurnAngle(turn);
		if (preciseRotation == null) {
			do {
				//Ugly, but Oh well... Crutches
				Vector3 newForward = agent.steeringTarget - transform.position;
				if (newForward.magnitude > .00001f)
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(newForward), Time.deltaTime * 2f);
				yield return null;
				oldTurn = turn;
				turn = GetTurnVal();
				if (turn.Abs() > oldTurn.Abs() && turn.Abs() > SharpTurn) {
					SetTurnAngle();
				}
			} while (turn.Abs() > maxSlightTurn);
		} else {
			do {
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(agent.steeringTarget - transform.position), Time.deltaTime * 5f);
			} while ((transform.rotation * Quaternion.Inverse( preciseRotation.Value)).eulerAngles.magnitude < 2f);
		}

		Debug.Log($"Turn finished, GetTurnVal(precise {preciseRotation.HasValue.ToString()}) : {GetTurnVal()}");
		SetTurnAngle();
		_gizmoTransforms.Add((transform.position, transform.rotation));
		currentTurnCoroutine = null;
	}

	private float SetTurnAngle() => SetTurnAngle(GetTurnVal());
	private float SetTurnAngle(float ang) {
		anim.SetFloat(TurnAngle, ang);
		_VHS_animValues.x = anim.GetFloat(TurnAngle);
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

	private void TurnTo(Quaternion rotation) {
		_gizmoTransforms.Add((transform.position, transform.rotation));
		if (currentTurnCoroutine != null)
			StopCoroutine(currentTurnCoroutine);
		currentTurnCoroutine = TurnUpdate(rotation);
		StartCoroutine(currentTurnCoroutine);
	}

	private IEnumerator currentTurnCoroutine;
	/// <summary>
	/// Called on last frame in animation clip for turning on spot
	/// So when we start turning on spot, we can start moving after full turn motion
	/// </summary>
	public void StopTurning() {
		Debug.Log("Animation signalled for step stop "+ (agentAtDestination && currentInteractableTarget != null ? "at destination, have interaction target" : ""));
		if (currentInteractableTarget == null)
			SetTurnAngle();
		if (agentIsMoving)
			StartLerpSpeedTo(WalkSpeed, .2f);
		if (agentAtDestination && currentInteractableTarget != null) {
			//Start Interaction
			//anim.SetBool(currentInteractableTarget.AnimationSetBools, true);
			StartAllignment(currentInteractableTarget);
			foreach (var v in currentInteractableTarget.AnimationSetValues.Where(i => i.HasValue(AnimationProperty.Time.EnterBegin))) {
				v.SetAnimatorValue(anim, AnimationProperty.Time.EnterBegin);
			}
		}
	}

	public void StartAllignment(IInteractable interactable) {
		if (curretnAllignment != null)
			StopCoroutine(curretnAllignment);
		curretnAllignment = Allignment();
		StartCoroutine(curretnAllignment);
	}

	private IEnumerator curretnAllignment;
	bool allowAllignment;
	float allignmentTime;
	public void AllowAllignmentFor(float seconds) {
		allowAllignment = true;
		allignmentTime = seconds;
	}
	public IEnumerator Allignment() {
		yield return new WaitUntil(()=>allowAllignment);
		float startTime = Time.time;
		Vector3 finalPosition = currentInteractableTarget.InteractionEndPoint;
		Quaternion finalRotation = currentInteractableTarget.InteractionEndRotation;
		bool done = false;
		while (!done) {
			var adjustedTime = (Time.time - startTime < allignmentTime ? Time.time - startTime : allignmentTime);
			transform.position = transform.position.LerpTo(finalPosition, adjustedTime / allignmentTime);
			transform.rotation = Quaternion.Lerp(transform.rotation, finalRotation, adjustedTime / allignmentTime);
			done = Time.time - startTime > allignmentTime;//to finish after time ran out
			yield return null;
		}

		curretnAllignment = null;
		allowAllignment = false;
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
				_VHS_animValues.y = anim.GetFloat(Speed);
				yield return null;
			}
			anim.SetFloat(Speed, speed);
			_VHS_animValues.y = anim.GetFloat(Speed);
			currentLerpSpeedTo = null;
			Debug.Log($"LerpSpeedTo {speed} for {seconds} finished Time.time:{Time.time}");
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
		Physics.Raycast(transform.position + transform.up * .5f, Vector3.down, out var hit, 1f, LayerMask.GetMask("NavigationTargets"));
		transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);


		if (ignoreInput) return;
		_VHS_animValues.z = anim.GetBool(Walk) ? 1 : 0;
		//Stop
		if (agentAtDestination && agentIsMoving) {
			_gizmoTransforms.Add((transform.position, transform.rotation));
			Debug.Log($"Target reached, stopping");
			anim.SetBool(Walk, false);
			StartLerpSpeedTo(0f, .1f);
			SetTurnAngle(0);
			agentIsMoving = false;
			if (currentInteractableTarget != null) {
				//Rotate towards
				TurnTo(currentInteractableTarget.InteractionEntryRotation);
			}
		}

		//Next target, turn towards, continue walk
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

		//Offcourse, restart Turning
		if (agentIsMoving && currentTurnCoroutine == null && GetTurnVal().Abs() > maxSlightTurn) {
			Debug.Log($"Offcourse agentMoving && notTurning && GetTurnVal() : {GetTurnVal()} > {maxSlightTurn}");
			RestartTurning();
		}

		//Slowing down
		if (agentIsMoving && NextCornerIsSharp || agent.steeringTarget == agent.destination) {
			var slowDownSpeed = agent.steeringTarget == agent.destination ? 0 : SlowWalkSpeed;
			if (currentSlowDownToByProximity != null && transform.position.DistanceTo(agent.steeringTarget) < SharpTurnSlowDownDistance)
				StartSlowDownToByProximity(slowDownSpeed);
		}

		agent.nextPosition = transform.position;
		lastSteeringTarget = agent.steeringTarget;
	}

	private void StartSlowDownToByProximity(float slowDownSpeed) {
		if (currentSlowDownToByProximity != null)
			StopCoroutine(currentSlowDownToByProximity);
		currentSlowDownToByProximity = SlowDownToByProximity(slowDownSpeed);
		StartCoroutine(currentSlowDownToByProximity);
	}
	IEnumerator currentSlowDownToByProximity;
	IEnumerator SlowDownToByProximity(float slowDownSpeed) {
		Debug.Log($"Slowing down, agent.steeringTarget == agent.destination, speed => {slowDownSpeed}");
		var proximity = ProximityToSharpCornerOrEnd();
		while (proximity > .01f) {
			anim.SetFloat(Speed, anim.GetFloat(Speed).LerpTo(slowDownSpeed, proximity));
			_VHS_animValues.y = anim.GetFloat(Speed);
			transform.position = transform.position.LerpTo(agent.steeringTarget, proximity);
			yield return null;
		}
		anim.SetFloat(Speed, slowDownSpeed);
		_VHS_animValues.y = anim.GetFloat(Speed);
		currentSlowDownToByProximity = null;
	}

	/* 
	 * ========================================================================================================
	 * Gizmo stuff
	 * ========================================================================================================
	 */

	[PlotValue(200,100)]
	public Vector3 _VHS_animValues = Vector3.zero;

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
			var stt = transform.InverseTransformPoint(agent.steeringTarget).normalized;
			if (stt.magnitude > .00001f)
				Gizmos.DrawLine(transform.position, transform.position + Quaternion.LookRotation(stt, transform.up) * transform.forward);
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
