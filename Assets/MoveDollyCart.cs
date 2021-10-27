using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveDollyCart : MonoBehaviour
{
	public CinemachineDollyCart cart;
	public PlayerInput input;
	public float amount = .2f;
	public float tweenTime = .5f;
	public Camera cam;
	public BoxCollider[] invisibleWalls;
	public float zzz;
	float lastTweenTime;

	private void OnEnable() {
		cam = cart.GetComponentInChildren<Camera>();
		input = new PlayerInput();
		input.Player.Enable();
	}
	private void Update() {
		if (!cam) return;
		var posx = input.Player.ScreenPointer.ReadVector2().x;
		zzz = posx;
		if (lastTweenTime < Time.time + tweenTime && (posx < 2 || posx > Screen.width - 2)) {
			LeanTween.value(gameObject, v => cart.m_Position = v, cart.m_Position, cart.m_Position + amount * (int)((posx + .1f) / Screen.width * 2 - 1), tweenTime);
			lastTweenTime = Time.time;
		}
	}
}
