using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomSMBehaviour : StateMachineBehaviour
{
	public int MaxRandom = 1;
	int RandomHash;
	private System.Random rng;
	public int currentLoopIndex = 0;
	public string path;
	private bool _initialized;

	private void OnEnable() {
		RandomHash = Animator.StringToHash("Random");
		rng = new System.Random();
	}

	// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!_initialized) Initialize(animator, stateInfo, layerIndex);
	}

	private void Initialize(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		SetRng(animator);
		currentLoopIndex = 0;
		_initialized = true;
	}

	private void SetRng(Animator animator) {
		animator.SetInteger(RandomHash, rng.Next(0, MaxRandom));
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ((stateInfo.normalizedTime) / stateInfo.length > currentLoopIndex) {
			currentLoopIndex++;
			SetRng(animator);
		}
	}
}
