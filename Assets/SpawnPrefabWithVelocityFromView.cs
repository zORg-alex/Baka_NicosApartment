using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnPrefabWithVelocityFromView : MonoBehaviour
{
	public InputAction screenPosition;
	public InputAction activation;

	public Rigidbody prefab;
	public float initialVelocity = 10f;
	public float initialTorque = 1f;

	Ray lastRay;

	public float life = 5f;

	private void OnEnable() {
		screenPosition = new InputAction("", InputActionType.Value, "<Mouse>/position");
		activation = new InputAction("", InputActionType.Value, "<Mouse>/leftButton");
		activation.performed += Activation_performed;
		screenPosition.Enable();
		activation.Enable();
	}
	private void OnDisable() {
		activation.Disable();
		screenPosition.Disable();
	}

	private void Activation_performed(InputAction.CallbackContext ctx) {
		var pos = screenPosition.ReadVector2();
		lastRay = Camera.main.ScreenPointToRay(pos);
		var dir = Camera.main.ScreenPointToRay(pos).direction;

		var pref = Instantiate(prefab,transform.position, Random.rotation);
		pref.AddTorque(Random.onUnitSphere * initialTorque, ForceMode.VelocityChange);
		pref.AddForce(dir * initialVelocity, ForceMode.VelocityChange);

		StartCoroutine(KillOnTImer(pref.gameObject));
	}

	private IEnumerator KillOnTImer(GameObject o) {
		yield return new WaitForSeconds(life);
		Destroy(o);
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = new Color(.6f, 1, .4f, .3f);
		Gizmos.DrawRay(lastRay);
	}
}
