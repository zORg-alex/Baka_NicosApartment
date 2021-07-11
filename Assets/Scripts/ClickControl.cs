using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static HiddenMonoBehaviours;

public class ClickControl : AssemblyReloadableMonoBehaviour, IOnMouseClick
{
	private PlayerInput input;

	public Transform MouseOverObject { get; private set; }
	public Vector3 MouseOverPoint { get; private set; }
	public UnityEvent<ClickControl> OnClick;

	public ClickControl() {
		Initialize = Init;
	}

	private void Init() {
		input = new PlayerInput();
		input.Enable();
		input.Player.Accept.performed += e => {
			if (enabled) {
				if (Physics.Raycast(Camera.main.ScreenPointToRay(input.Player.ScreenPointer.ReadVector2()), out var hit)) {
					if (hit.collider != null) {
						MouseOverObject = hit.transform;
						MouseOverPoint = hit.point;
					}
				}
			}
		};
	}
}

internal interface IOnMouseClick {
	Vector3 MouseOverPoint { get; }
	Transform MouseOverObject { get; }
}