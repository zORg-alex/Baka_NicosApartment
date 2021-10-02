using FIMSpace.Basics;
using FIMSpace.GroundFitter;
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
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Aud_Footsteps))]
[ExecuteInEditMode]
[SelectionBase]
public class GeorgeAnimationControllerScript : MonoBehaviour {
	public PlayerInput input;

	//Values to set Animator propeerties or interpret them. Set in Inspector
	#region animatorSettingFields
	/// <summary>
	/// 90 deg in Animator turn animations
	/// </summary>
	[HorizontalGroup("Animator",150)]
	[FoldoutGroup("Animator/Animator Turn settings"), LabelWidth(100)]
	public float turn90Deg = 1.5f;
	[FoldoutGroup("Animator/Animator Turn settings"), LabelWidth(100)]
	public float SharpTurn = 1.5f;
	[FoldoutGroup("Animator/Animator Turn settings"), LabelWidth(100)]
	public float maxSlightTurn = .5f;

	[HorizontalGroup("Animator")]
	[FoldoutGroup("Animator/Animator Speed settings")]
	public float WalkSpeed = 1.5f;
	[FoldoutGroup("Animator/Animator Speed settings")]
	public float SharpTurnSlowDownDistance = .5f;
	[FoldoutGroup("Animator/Animator Speed settings")]
	public float SlowWalkSpeed = .7f;
	#endregion

	public Vector3 destination;
	private Vector3 oldTarget;

	private void OnEnable() {
		SetUpInputActions();
		IndexAnimParameters();
		SetUpAnimationClips();
		SetUpAgent();
	}
	private void OnDisable() {
		input.Player.Accept.performed -= Clicked;
	}

	public NavMeshAgent agent;
	
	private void SetUpAgent() {
		agent = GetComponent<NavMeshAgent>();
		agent.updatePosition = true;
		agent.updateRotation = true;
		agent.updateUpAxis = true;
		agent.Warp(transform.position);
		agent.destination = transform.position;
	}
	string _clip_Stand = "Stand";
	string _clip_Move = "Move";
	string _clip_Sit_Enter = "Sit_Enter";
	string _clip_Sit_Exit = "Sit_Exit";
	FAnimationClips clips;

	private void SetUpAnimationClips() {
		clips = new FAnimationClips(anim);
		clips.Add(_clip_Stand);
		clips.Add(_clip_Move);
		clips.Add(_clip_Sit_Enter);
		clips.Add(_clip_Sit_Exit);
	}

	private void SetUpInputActions() {
		input = new PlayerInput();
		input.Player.Alternative.performed += Clicked;
		input.Player.Enable();
	}

	Animator anim;
	#region Animator Properties indexed
	int Sit;
	int Walk;
	int Speed;
	int Turn;
	int TurnAngle;
	int SitMirror;
	#endregion

	private void IndexAnimParameters() {
		anim = GetComponent<Animator>();
		Sit = Animator.StringToHash("Sit");
		Walk = Animator.StringToHash("Walk");
		Speed = Animator.StringToHash("Speed");
		Turn = Animator.StringToHash("Turn");
		TurnAngle = Animator.StringToHash("TurnAngle");
		SitMirror = Animator.StringToHash("SitMirror");
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

	private float GetTurnVal() {
		var direction = transform.InverseTransformPoint(agent.steeringTarget.Horizontal()).normalized;
		return ConvertDegAngleToAnimAngle(ConvertDirectionToDegAngle(direction));
	}

	private float ConvertDirectionToDegAngle(Vector3 direction) {
		return Quaternion.FromToRotation(Vector3.forward, (direction)).eulerAngles.y;
	}

	public float ConvertDegAngleToAnimAngle(float ang) {
		return (ang < 180 ? ang : -360 + ang) / (90 / turn90Deg);
	}

	/// <summary>
	/// Called on last frame in animation clip for turning on spot
	/// So when we start turning on spot, we can start moving after full turn motion
	/// <para/> Doing a simple culling for future calls on current frame with <see cref="stoppedTurning"/> bool.
	/// </summary>
	public void StopTurning() => stoppedTurning = true;
	private bool stoppedTurning;
	public void StopTurningTriggered() {
		stoppedTurning = false;



	}

	/// <summary>
	/// Set in <see cref="Update"/>
	/// </summary>
	public bool agentIsMoving;
	public bool isTurning;
	/// <summary>
	/// Disables <see cref="Update"/>
	/// </summary>
	/// 
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
	private float _turnStart;
	private Quaternion _turnStartRotation;
	private bool _lookAtTurning;
	public float turnTIme = .5f;

	private void Update() {
		if (stoppedTurning) StopTurningTriggered();

		//Look at steering target, do it before, so not to do it on time 0
		if (_lookAtTurning) {

			Vector3 direction = (agent.steeringTarget - transform.position).normalized;
			if (direction != Vector3.zero) {
				Quaternion newRotation = Quaternion.Lerp(_turnStartRotation,
					Quaternion.LookRotation(direction, Vector3.up),
					Mathf.Min((Time.time - _turnStart) / turnTIme, 1f));

				transform.rotation = newRotation;
			}
			if (Time.time > _turnStart + turnTIme) _lookAtTurning = false;

			//Allign character to a terrain height; Raycast from hips to ground
			//if (!groundFitter.enabled) {
			//	Physics.Raycast(transform.position + transform.up * .5f, Vector3.down, out var hit, 1f, LayerMask.GetMask("NavigationTargets"));
			//	transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
			//}
		}

		//New target, start turning
		if (agent.steeringTarget != lastSteeringTarget) {
			//Don't want to update every frame, since it's not going to change drastically
			NextCornerIsSharp = nextCornerIsSharp();
			lastSteeringTarget = agent.steeringTarget;

			_turnStart = Time.time;
			_turnStartRotation = transform.rotation;
			_lookAtTurning = (agent.steeringTarget - transform.position).magnitude > .3f;
		}

		//Start moving
		if (!agentIsMoving && !agentAtDestination) {
			agentIsMoving = true;
			clips.CrossFade(_clip_Move, .5f);
			agent.speed = WalkSpeed;
		}
		//Stop moving
		if (agentIsMoving && agentAtDestination) {
			agentIsMoving = false;
			if (currentInteractableTarget != null) {
				if (currentInteractableTarget.AnimationSetValues.FirstOrDefault()?.name == "Sit") {
					clips.CrossFade(_clip_Sit_Enter);
				}
			} else
				clips.CrossFade(_clip_Stand, .5f);
		}

		//Slowing down
		if (agentIsMoving && NextCornerIsSharp || agent.path.corners.Length == 1) {
			var slowDownSpeed = agent.path.corners.Length == 1 ? 0 : SlowWalkSpeed;
			if (currentSlowDownToByProximity != null && transform.position.DistanceTo(agent.steeringTarget) < SharpTurnSlowDownDistance)
				StartSlowDownToByProximity(slowDownSpeed);
		}
		//agent.nextPosition = transform.position;
		anim.SetFloat(Speed, agent.velocity.magnitude);
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
		var exit = false;
		var corner = agent.steeringTarget;
		while (!exit) {
			var proximity = ProximityToSharpCornerOrEnd();
			anim.SetFloat(Speed, anim.GetFloat(Speed).LerpTo(slowDownSpeed, proximity));
			_VHS_animValues.y = anim.GetFloat(Speed);
			transform.position = transform.position.LerpTo(agent.steeringTarget, proximity);
			exit = proximity < .1f || corner != agent.steeringTarget;
			yield return null;
		}
		agent.speed = slowDownSpeed;
		_VHS_animValues.y = anim.GetFloat(Speed);
		currentSlowDownToByProximity = null;
	}

	/* 
	 * ========================================================================================================
	 * Gizmo stuff
	 * ========================================================================================================
	 */

	[Space(30)]
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
		if (isTurning) {
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
