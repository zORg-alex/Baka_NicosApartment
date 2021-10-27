using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurnOffLights : MonoBehaviour
{
    public InputAction action;
    public List<Light> lights;
    void OnEnable()
    {
        action.Enable();
		action.performed += Action_performed;
    }

	private void Action_performed(InputAction.CallbackContext obj) {
		foreach (var light in lights) {
            light.enabled = !light.enabled;
		}
	}
}
