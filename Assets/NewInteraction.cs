using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

[ExecuteInEditMode]
public class NewInteraction : SerializedMonoBehaviour
{
	[ShowInInspector, OdinSerialize]
	public Vector3 InteractionEntryPointLocal { get; private set; }
	public Vector3 InteractionEntryPoint => transform.TransformPoint(InteractionEntryRotationLocal * InteractionEntryPointLocal);

	[ShowInInspector, OdinSerialize]
	public Quaternion InteractionEntryRotationLocal { get; private set; }
	public Quaternion InteractionEntryRotation => transform.rotation * InteractionEntryRotationLocal;

	[ShowInInspector, OdinSerialize]
	public Vector3 InteractionEndPointLocal { get; private set; }
	public Vector3 InteractionEndPoint => transform.TransformPoint(InteractionEndRotationLocal * InteractionEndPointLocal);

	[ShowInInspector, OdinSerialize]
	public Quaternion InteractionEndRotationLocal { get; private set; }
	public Quaternion InteractionEndRotation => transform.rotation * InteractionEndRotationLocal;

	public Animator _animator;
	public AnimatorController _animContr;
	public List<string> parameters;
	public List<string> behaviours;
	public List<string> states;

	private void OnEnable() {
		parameters = _animator.parameters.Select(p => p.name).ToList();
		var fanim = new FIMSpace.Basics.FAnimator(_animator);
		behaviours = _animator.GetBehaviours<StateMachineBehaviour>().Select(b=>$"{b.name} {b.GetType().Name}").ToList();
		states = _animContr.layers[0].stateMachine.states.Select(s => s.state.name).ToList();

	}
}
