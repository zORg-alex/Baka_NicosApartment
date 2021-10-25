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
	public float Turn90Deg = 1.5f;
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

	public float StopToWalkTime = .3f;
	public float SlowTurnDistance90Deg = 1.5f;
	#endregion

	public NavMeshAgent agent;
	public Vector3 destination;
	private Vector3 oldTarget;
	Vector3 lastSteeringTarget;

	[NonSerialized]
	bool initialized;
	private void OnEnable() {
		if (!initialized) {
			SetUpInputActions();
			IndexAnimParameters();
			SetUpAgent();
			initialized = true;
		}
	}
	private void SetUpAgent() {
		agent = GetComponent<NavMeshAgent>();
		agent.updatePosition = true;
		agent.updateRotation = true;
		agent.updateUpAxis = true;
		agent.Warp(transform.position);
		agent.updatePosition = false;
		agent.updateRotation = false;
		//agent.destination = transform.position;
		lastSteeringTarget = agent.steeringTarget;
		try {
			if (Application.isPlaying)
				agent.isStopped = true;
		}catch (Exception) { }

	}


	private void SetUpInputActions() {
		if (Application.isPlaying && input == null) {
			input = new PlayerInput();
			input.Player.Alternative.performed += Clicked;
			input.Player.Enable();
		}
	}

	Animator anim;
	#region Animator Properties indexed
	int _sit;
	int _walk;
	int _speed;
	int _turn;
	int _turnAngle;
	int _strafe;
	int _sitMirror;
	int _resetWalk;
	#endregion

	private void IndexAnimParameters() {
		anim = GetComponent<Animator>();
		_sit = Animator.StringToHash("Sit");
		_walk = Animator.StringToHash("Walk");
		_speed = Animator.StringToHash("Speed");
		_turn = Animator.StringToHash("Turn");
		_turnAngle = Animator.StringToHash("TurnAngle");
		_strafe = Animator.StringToHash("Strafe");
		_sitMirror = Animator.StringToHash("SitMirror");
		_resetWalk = Animator.StringToHash("ResetWalk");
	}
	private void TurnAngle(float turnAngle) {
		_VHS_animValues.x = turnAngle;
		anim.SetFloat(_turnAngle, turnAngle);
	}
	private void Speed(float speed) {
		_VHS_animValues.y = speed;
		agent.speed = speed;
		anim.SetFloat(_speed, speed);
	}
	private void Walk(bool walk) {
		_VHS_animValues.z = walk ? 1 : 0;
		anim.SetBool(_walk, walk);
	}
	private void Strafe(float strafe) {
		anim.SetFloat(_strafe, strafe);
	}
	void ResetWalk() {
		anim?.SetTrigger(_resetWalk);
	}

	private bool stoppedTurning;
	/// <summary>
	/// Called on last frame in animation clip for turning on spot
	/// So when we start turning on spot, we can start moving after full turn motion
	/// <para/> Doing a simple culling for future calls on current frame with <see cref="stoppedTurning"/> bool.
	/// </summary>
	public void StopTurning() {
		Debug.Log("StoppedTurning");
		if (!stoppedTurning) {
			stoppedTurning = true;
			StartCoroutine(StopTurningTriggered());
		}

		IEnumerator StopTurningTriggered() {
			yield return null;
			stoppedTurning = false;
		}
	}
	bool turnMidpoint;
	/// <summary>
	/// Called in the middle of turning animation to start movement
	/// </summary>
	public void TurnMidpoint() {
		Debug.Log("TurnMidpoint");
		if (!turnMidpoint) {
			turnMidpoint = true;
			StartCoroutine(TurnMidpointTriggered());
		}
		IEnumerator TurnMidpointTriggered() {
			yield return null;
			turnMidpoint = false;
		}
	}

	private IInteractable currentInteractableTarget;
	private void Clicked(InputAction.CallbackContext obj) {
		Debug.Log($"Clicked, setting destination, currentInteractableTarget? {currentInteractableTarget != null}");
		if (agent != null) {
			var ray = Camera.main.ScreenPointToRay(input.Player.ScreenPointer.ReadVector2());
			if (Physics.Raycast(ray, out var hit)) {
				if (hit.collider?.GetComponent<IInteractable>() is IInteractable interactable) {
					//If interactable hit, set it or set to null
					SetDestination(interactable.InteractionEntryPoint);
					currentInteractableTarget = interactable;
				} else if (NavMesh.SamplePosition(hit.point, out var navhit, 4, NavMesh.AllAreas)) {
					//If interactable was set previously, means it need to be exited
					SetDestination(navhit.position);
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
	public void SetDestination(Vector3 destination) {
		agent.isStopped = false;
		agent.destination = destination;
		isMoving = true;
	}
	public void StartTurn() {
		if (_turnCR != null)
			StopCoroutine(_turnCR);
		_turnCR = TurnCR(Quaternion.FromToRotation(Vector3.forward, transform.InverseTransformPoint(agent.steeringTarget).normalized));
		StartCoroutine(_turnCR);
	}
	private IEnumerator _turnCR;
	IEnumerator TurnCR(Quaternion rotation) {
		Debug.Log($"TurnCR started {rotation}");
		var turn = ConvertDegAngleToAnimAngle(rotation.eulerAngles.y);
		var dist = Vector3.Distance(transform.position, agent.steeringTarget);
		//Stop if close target & sharp turn
		if (dist < SlowTurnDistance90Deg * turn.Abs() / Turn90Deg) {
			Debug.Log($"TurnCR dist:{dist} < {SlowTurnDistance90Deg * turn / Turn90Deg} slowdown, before close target & sharp turn");
			TweenSpeedToFor(0f, StopToWalkTime);
			//wait = Time.time + StopToWalkTime;
			yield return new WaitForSeconds(StopToWalkTime);
		}
		ResetWalk();
		TurnAngle(turn);
		yield return null;
		var curTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
		//anim.MatchTarget(Vector3.zero, rotation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.zero, 1), curTime , .999f, true);
		if (!stoppedTurning || !turnMidpoint)
			yield return new WaitUntil(() => stoppedTurning || turnMidpoint);
		Debug.Log($"TurnCR stopped rotating, TurnAngle = 0, Speed => 0");
		TweenSpeedToFor(WalkSpeed, StopToWalkTime);
		if (!stoppedTurning)
			yield return new WaitUntil(() => stoppedTurning);
		TurnAngle(0f);
		//correct course if necessary
		Debug.Log($"TurnCR start correcting course");
		while (!agentAtDestination) {
			turn = GetTurnVal();
			if (turn.Abs() > maxSlightTurn) {
				var animTime = anim.GetCurrentAnimatorClipInfo(0).FirstOrDefault().clip?.averageDuration ?? 0f;
				TurnAngle(turn);
				yield return new WaitForSeconds(animTime);
			} else {
				TurnAngle(0);
				yield return null;
			}
		}
		//SetTurnAngle(0f);
		_turnCR = null;
	}

	private void TurnTo(Quaternion rotation, Action continueWith = null) {
		Debug.Log($"TurnTo: rotation:{rotation}, continueWith:{continueWith != null}");
		if (_curTurnToCR != null) StopCoroutine(_curTurnToCR);
		_curTurnToCR = TurnToCR(rotation, continueWith);
		StartCoroutine(_curTurnToCR);

		IEnumerator TurnToCR(Quaternion rotation, Action continueWith = null) {
			var turn = ConvertDegAngleToAnimAngle((rotation * transform.rotation.Inverted()).eulerAngles.y);
			var walk = anim.GetBool(_walk);
			if (!walk) Walk(true);
			TurnAngle(turn);
			yield return new WaitUntil(() => stoppedTurning);
			Debug.Log($"TurnTo: stoppedTurning, walk => false");
			if (!walk) Walk(false);
			continueWith?.Invoke();

			_curTurnToCR = null;
		}
	}
	IEnumerator _curTurnToCR;

	private float GetTurnVal() {
		var direction = transform.InverseTransformPoint(agent.steeringTarget.Horizontal()).normalized;
		return ConvertDegAngleToAnimAngle(ConvertDirectionToDegAngle(direction));
	}

	private float ConvertDirectionToDegAngle(Vector3 direction) {
		return Quaternion.FromToRotation(Vector3.forward, (direction)).eulerAngles.y;
	}

	public float ConvertDegAngleToAnimAngle(float ang) {
		return (ang < 180 ? ang : -360 + ang) / (90 / Turn90Deg);
	}

	public void StopMoving() {
		agent.destination = transform.position;
		agent.isStopped = true;
		isMoving = false;
	}

	public bool isMoving;

	public bool _nextCornerIsSharp;
	public bool NextCornerIsSharp() => agent.path.corners.Length > 1 ? ConvertDegAngleToAnimAngle(ConvertDirectionToDegAngle(transform.InverseTransformPoint(agent.path.corners[1] - agent.path.corners[0]).normalized)) > SharpTurn : false;

	/// <summary>How close to a sharp corner if there are > 1</summary>
	/// <returns>[0..1]</returns>
	public float ProximityToSharpCornerOrEnd() {
		var lastTargetOrSharpTurn = _nextCornerIsSharp;
		if (agent.steeringTarget == agent.destination) lastTargetOrSharpTurn = true;

		return lastTargetOrSharpTurn ? 1 - transform.position.DistanceTo(agent.steeringTarget) / SharpTurnSlowDownDistance : 0;
	}

	public bool agentAtDestination => transform.position.DistanceTo(agent.destination) < agent.stoppingDistance;

	bool stoppedMovement;

	private void Update() {
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		//Steering target changed or
		if (isMoving && lastSteeringTarget != agent.steeringTarget) {
			Debug.Log($"Update: SteeringTarget changed, walk = true, StartTurn, next corner is sharp {_nextCornerIsSharp}");
			Walk(true);
			StartTurn();
			_nextCornerIsSharp = NextCornerIsSharp();
		}
		//Course correction is done in TurnCR
		if (isMoving && agentAtDestination && _curTurnToCR == null) {
			Debug.Log($"Update: isMoving && agentAtDestination => stop moving");
			isMoving = false;
			TurnAngle(0f);
			Strafe(0f);
			if (_curSlowDownToByProximityCR != null) {
				StopCoroutine(_curSlowDownToByProximityCR);
				_curSlowDownToByProximityCR = null;
			}
			var t = TweenSpeedToFor(0f, StopToWalkTime);
			t.setOnComplete(() => {
				if (agentAtDestination) {
					Walk(false);
					stoppedMovement = true;
				}
			});
		}
		if (stoppedMovement && _curTurnToCR == null) {
			Debug.Log($"Update: after stopped moving, turn to interactive {currentInteractableTarget != null}");
			stoppedMovement = false;
			if (currentInteractableTarget != null) {
				TurnTo(currentInteractableTarget.InteractionEntryRotation, () => StartCoroutine(EnterInteractionCR()));
			}
		}
		if (isMoving && _curSlowDownToByProximityCR == null &&
			transform.position.DistanceTo(agent.steeringTarget) < SharpTurnSlowDownDistance) {
			Debug.Log($"Update: slow down by proximity");
			SlowDownToByProximity(agent.path.corners.Length == 1 ? 0 : SlowWalkSpeed);
		}

		transform.position = transform.position.SetY(Physics.RaycastAll(new Ray(transform.position + Vector3.up, Vector3.down), 3, 64)?.Max(h=>h.point.y) ?? 0f);
		agent.nextPosition = transform.position;
		lastSteeringTarget = agent.steeringTarget;
	}

	private LTDescr TweenSpeedToFor(float speed, float time) {
		return LeanTween.value(gameObject, val => Speed(val), anim.GetFloat(_speed), speed, time);
	}

	IEnumerator EnterInteractionCR() {
		if (currentInteractableTarget != null) {
			currentInteractableTarget.ApplyFor(anim, AnimationProperty.Time.EnterBegin);
			yield return new WaitUntil(() => startMatchTarget);
			if (currentInteractableTarget != null)
				anim.MatchTarget(currentInteractableTarget.InteractionEndPoint, currentInteractableTarget.InteractionEndRotation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 1f), 0f, 1f);
		}
	}
	private bool startMatchTarget;
	private void StartMatchTarget() {
		if (startMatchTarget) return;
		startMatchTarget = true;
		StartCoroutine(StartMatchTargetCR());
		IEnumerator StartMatchTargetCR() {
			yield return null;
			startMatchTarget = false;
		}
	}

	private void SlowDownToByProximity(float slowDownSpeed) {
		Debug.Log($"SlowDownToByProximity: to {slowDownSpeed}");
		if (_curSlowDownToByProximityCR != null)
			StopCoroutine(_curSlowDownToByProximityCR);
		_curSlowDownToByProximityCR = SlowDownToByProximityCR(slowDownSpeed);
		StartCoroutine(_curSlowDownToByProximityCR);

		IEnumerator SlowDownToByProximityCR(float slowDownSpeed) {
			var exit = false;
			var corner = agent.steeringTarget;
			//anim.MatchTarget(currentInteractableTarget?.InteractionEntryPoint ?? agent.steeringTarget,
			//	currentInteractableTarget?.InteractionEndRotation ?? Quaternion.identity,
			//	AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, currentInteractableTarget != null ? 1 : 0), 0f, 1f, false);
			while (!exit) {
				var proximity = ProximityToSharpCornerOrEnd();
				Speed(anim.GetFloat(_speed).LerpTo(slowDownSpeed, proximity));
				exit = agentAtDestination || corner != agent.steeringTarget;
				yield return null;
			}
			_curSlowDownToByProximityCR = null;
		}
	}
	IEnumerator _curSlowDownToByProximityCR;

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
