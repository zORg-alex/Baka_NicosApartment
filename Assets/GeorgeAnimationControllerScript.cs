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
		input.Player.Alternative.performed += Clicked;
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
	public void StopTurningTrigger() {
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
		if (stoppedTurning) StopTurningTrigger();
		if (agent.steeringTarget != lastSteeringTarget) {
			//Don't want to update every frame, since it's not going to change drastically
			NextCornerIsSharp = nextCornerIsSharp();
			lastSteeringTarget = agent.steeringTarget;
		}

		//Allign character to a terrain height; Raycast from hips to ground
		Physics.Raycast(transform.position + transform.up * .5f, Vector3.down, out var hit, 1f, LayerMask.GetMask("NavigationTargets"));
		transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);


		if (ignoreInput) return;

		agent.nextPosition = transform.position;
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
