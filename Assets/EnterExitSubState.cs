using UnityEngine;

public class EnterExitSubState : SubState {
	public event StateMachineBehaviourMessage onStateEnter;
	public event StateMachineBehaviourMessage onStateExit;
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		onStateEnter?.Invoke(animator, stateInfo, layerIndex);
	}
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		onStateExit?.Invoke(animator, stateInfo, layerIndex);
	}
}