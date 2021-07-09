/// <summary>
/// Here will go all clicks and other gestures recognition
/// </summary>
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class ClickScript : MonoBehaviour {

	public InputAction leftClick;
	public InputAction rightClick;
	public InputAction middleClick;
	public InputAction mousePos;

	public bool LeftClick ;//{get; private set;}
	public bool RightClick {get; private set;}
	public bool MiddleClick {get; private set;}
	public bool LongLeftClick ;//{get; private set;}
	public bool LongRightClick {get; private set;}
	public bool LongMiddleClick {get; private set;}
	public bool LongLeftHold ;//{get; private set;}
	public bool LongRightHold {get; private set;}
	public bool LongMiddleHold {get; private set;}
	public bool LeftClickNDdrag ;//{get; private set;}
	public bool RightClickNDdrag {get; private set;}
	public bool MiddleClickNDdrag {get; private set;}
	public Vector3 ClickPoint {get; private set;}
	public Transform clickedObject;
	public Transform ClickedObject
	{
		get{
			First = false;
			return clickedObject;}
		private set{
			clickedObject = value;
			First = true;
		}
	}
	public bool First; //first access to clicked object

	public Vector3 MouseOverPoint { get; private set; }
	public Transform MouseOverObject { get; private set; }

	float timer=0;
	bool clickRTemp;
	bool clickLTemp;
	bool clickMTemp;
	float cRtimer=0;
	float cLtimer=0;
	float cMtimer=0;
	Vector2 mousePosOld;
	private void Awake() {
		leftClick = new InputAction("LMB", InputActionType.Button, "<Mouse>/leftButton");
		rightClick = new InputAction("RMB", InputActionType.Button, "<Mouse>/rightButton");
		middleClick = new InputAction("MMB", InputActionType.Button, "<Mouse>/middleButton");
		mousePos = new InputAction("position", InputActionType.Value, "<Mouse>/position");
		leftClick.Enable();
		rightClick.Enable();
		middleClick.Enable();
		mousePos.Enable();
	}
	/// <summary>
	/// OnEnable runs on assembly reload if start is present
	/// </summary>
	private void Start() { }
	private void OnEnable() {
		Awake();
	}

	// Update is called once per frame
	void Update () {
		LeftClick = false;
		RightClick = false;
		MiddleClick = false;
		LongLeftClick = false;
		LongRightClick = false;
		LongMiddleClick = false;
		LeftClickNDdrag = false;
		RightClickNDdrag = false;
		MiddleClickNDdrag = false;
		LongLeftHold = false;
		LongRightHold = false;
		LongMiddleHold = false;
		var mousePosition = mousePos.ReadValue<Vector2>();

		if (leftClick.ReadValue<float>() > 0) {
			//Debug.Log("Click");
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				ClickedObject = hit.transform;
				ClickPoint = hit.point;
				//TODO Select Object (Outline)
			}
			clickLTemp = true;
			mousePosOld = mousePosition;

		}
		if (rightClick.ReadValue<float>() > 0) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				ClickedObject = hit.transform;
				ClickPoint = hit.point;
				//TODO Select Object (Outline)
			}
			clickRTemp = true;
			mousePosOld = mousePosition;
		}
		if (middleClick.ReadValue<float>() > 0) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				ClickedObject = hit.transform;
				ClickPoint = hit.point;
				//TODO Select Object (Outline)
			}
			clickMTemp = true;
			mousePosOld = mousePosition;
			
		}
		timer+=Time.deltaTime;
		if (timer>0.1f) {
			timer=0;
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (Physics.Raycast(ray, out hit)) {
				if (hit.rigidbody != null) {
					MouseOverObject = hit.transform;
					MouseOverPoint = hit.point;
					//TODO Object Highlight
				}
			}
		}
		if (clickLTemp){
			cLtimer+=Time.deltaTime;
			if (leftClick.ReadValue<float>() > 0) {
				clickLTemp = false;
				//Debug.Log((Input.mousePosition-mousePosOld).magnitude);
				if (cLtimer<0.5){
					LeftClick = true;
					//Debug.Log("Click");
				}
				else{
					LongLeftClick = true;
					//Debug.Log("Long");
				}
				//Debug.Log(cLtimer);
				cLtimer = 0;
			}
			else {
				if ((mousePosition - mousePosOld).magnitude>5){
					LeftClickNDdrag = true;
					//Debug.Log("Drag");
				}
				else {
					LongLeftHold = true;
				}
			}
		}
		if (clickRTemp){
			cRtimer+=Time.deltaTime;
			if (rightClick.ReadValue<float>() > 0) {
				clickRTemp = false;
				//Debug.Log((Input.mousePosition-mousePosOld).magnitude);
				if (cRtimer<0.5){
					RightClick = true;
				}
				else{
					LongRightClick = true;
				}
				//Debug.Log(cRtimer);
				cRtimer = 0;
			}
			else {
				if ((mousePosition - mousePosOld).magnitude>5){
					RightClickNDdrag = true;
				}
				else {
					LongRightHold = true;
				}
			}
		}
		if (clickMTemp){
			cMtimer+=Time.deltaTime;
			if (middleClick.ReadValue<float>() > 0) {
				clickMTemp = false;
				if (cMtimer<0.5){
					MiddleClick = true;
				}
				else{
					LongMiddleClick = true;
				}
				cMtimer = 0;
			}
			if ((mousePosition - mousePosOld).magnitude>5){
				MiddleClickNDdrag = true;
			}
			else {
				LongMiddleHold = true;
			}
		}
	}

}