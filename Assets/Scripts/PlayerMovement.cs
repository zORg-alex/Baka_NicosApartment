using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour {
	PlayerInput playerInput;
	public ClickScript Mouse;			//Connect to object holding Click(Gestures) script
	Vector3 target = new Vector3();		//where to go
	Vector3 targetLocal = new Vector3();
	bool allign = false;				//true if final allignment should be applied
	Transform targetDir;				//final direction
	bool move = false;					//if performs any movements
	bool preturning = false;			//First turn towards dest
	bool walking = false;				//Walk towards dest
	bool aftturning = false;			//Turn after if allign
	bool holdTurnCalc = false;			//Helps not to ruin first calculation during Turn anim.
	bool sitAfterMovement = false;


	public float turnSmoothing = 15f;
	public float speedDampTime = 0.1f;
	public float runSpeed = 1f;
	public float walkSpeed = 0f;

	public float dist;

	//Hashes for quickier access to animator
	int SpeedFloat;
	int TurnAngFloat;
	int SitBool;
	int WalkBool;
	int ChinBool;
	int HandsBool;
	int TurnBool;
	int SitMirror;
	
	private Animator anim;

	void Awake()
	{
		playerInput = new PlayerInput();
		playerInput.Enable();
		SpeedFloat = Animator.StringToHash("Speed");
		TurnAngFloat = Animator.StringToHash("TurnAngle");
		SitBool = Animator.StringToHash("Sit");
		WalkBool = Animator.StringToHash ("Walk");
		ChinBool = Animator.StringToHash ("Chin");
		HandsBool = Animator.StringToHash ("Hands");
		TurnBool = Animator.StringToHash ("Turn");
		SitMirror = Animator.StringToHash ("SitMirror");

		anim = GetComponent<Animator>();
	}
	private void Start() { }
	private void OnEnable() {
		playerInput = new PlayerInput();
		playerInput.Enable();
	}

	void Update()
	{
		Vector2 dir = playerInput.Player.Move.ReadVector2();
		bool sneak = playerInput.Player.Careful.ReadBool();
		bool use = playerInput.Player.Careful.ReadBool();
		bool run = playerInput.Player.Fast.ReadBool();
		bool jump = playerInput.Player.Jump.ReadBool();

		//Debug.Log(Mouse.LeftClick);
		if (Mouse.LeftClick && Mouse.First) {   //1. if clicked
			if (Mouse.ClickPoint.y>-0.1 && Mouse.ClickPoint.y<0.1){ //2. if floor then set up to go there
				target = Mouse.ClickPoint;
				targetLocal = transform.InverseTransformDirection(target);
				allign = false;
				move = true;
				walking = true;
				anim.SetBool(SitBool,false);//Reset state to Stand Idle
			}
			if (Mouse.ClickedObject.GetComponent<ChairScript>()!=null){ // 3. if Chair, then set up to go sit
				ChairScript chair = Mouse.ClickedObject.GetComponent<ChairScript>();
				allign = true;
				target = chair.SeatPoint.position;
				targetLocal = transform.InverseTransformDirection(target);
				targetDir = chair.SeatPoint;
				move = true;
				walking = true;

			}
			target.y = 0;
		}

		if (move) {
			Vector3 targetDirRel = target - transform.position;
			dist = targetDirRel.magnitude;

			if (dist < 0.03f) { //7. Stop if reached or start turn if allign
				//Debug.Log("StopDist");
				anim.SetBool(WalkBool,false);
				walking = false;	//stop walking start alligning
				if (allign) {
					aftturning = true;
					allign = false;
				}
			}
			if (!holdTurnCalc){
				if (preturning) {	//5. if we decided to preturn, then start animation and skip to movement towards
					// since AnimationController will not allow to go anywhere until Turn ends, it will work
					float angle = Vector3.Angle(transform.forward,targetDirRel);
					if (targetLocal.x<0) angle *= -1;
					Debug.Log("Pre "+ angle+"\t\tdist: "+dist);
					anim.SetFloat(TurnAngFloat,angle);
					anim.SetBool(TurnBool,true);
					holdTurnCalc = true;
					preturning = false;
					walking = true;
				}
				else if (walking) {	
					// 4. if we are going somewhere, then decide if we need to preturn
					// It is here just in case we did not managed to turn quick enough
					Quaternion q = new Quaternion();
					q.SetLookRotation(targetDirRel,Vector3.up);
					float angle = Quaternion.Angle(q,transform.rotation);
					if (targetLocal.x<0) angle *= -1;

					angle = angle>180? 180 - angle : angle;
					//Debug.Log("Walk "+angle+"\t\tdist: "+dist);
					if (Mathf.Abs(angle)>60){
						//Debug.Log("Angle Error "+angle+"\t\tdist: "+dist);
						//Start turning prior movement
						preturning = true;
						walking = false;
						anim.SetFloat(SpeedFloat,0);
						anim.SetBool(WalkBool,false);
					}
					else {//6. Go Forward
						//Setup speed and queue walk animation
						anim.SetBool(WalkBool,true);
						
						float speed = walkSpeed*ATanFunc((targetDirRel.magnitude)*7);
						Rotating(targetDirRel);
						anim.SetFloat(SpeedFloat,speed);
					}
				}
				else if (aftturning) {			//8. Allign
					//Start Turn Animation to allign
					float angle = Quaternion.Angle(targetDir.rotation,transform.rotation);
					if (transform.InverseTransformDirection(targetDir.forward).x<0) angle *= -1;
					//Debug.Log("Aft "+angle);

					anim.SetFloat(TurnAngFloat,angle);
					anim.SetBool(TurnBool,true);
					holdTurnCalc = true;
					aftturning = false;
					if (Mouse.ClickedObject.GetComponent<ChairScript>()!=null) sitAfterMovement = true;
				}
				else if (sitAfterMovement) {
					sitAfterMovement = false;
					if (Mouse.ClickedObject.GetComponent<ChairScript>()!=null){
						anim.SetBool(SitBool,true);
						if (Mouse.ClickedObject.GetComponent<ChairScript>().Mirror){
							anim.SetBool(SitMirror,true);
						}
						else{
							anim.SetBool(SitMirror,false);
						}
					}
				}
				else {
					move = false;
				}

			}
		}
	}

	float ATanFunc(float x) { return Mathf.Atan(x)/Mathf.Atan(10); } //A good function wich drops to 0 pretty abruptly

	void Rotating (float horizontal, float vertical) {
		Rotating ( new Vector3 (horizontal, 0f, vertical) );
	}
	void Rotating (Vector3 targetDirection) {
		Rotating (Quaternion.LookRotation(targetDirection, Vector3.up));
	}
	void Rotating (Quaternion targetRotation)
	{
		// Create a new vector of the horizontal and vertical inputs.
		//Vector3 targetDirection = new Vector3(horizontal, 0f, vertical);
		// Create a rotation based on this new vector assuming that up is the global y axis.
		//Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);		
		//float yrot = Camera.main.transform.eulerAngles.y;
		//targetRotation.eulerAngles += new Vector3(0,yrot,0);
		// Create a rotation that is an increment closer to the target rotation from the player's rotation.
		Quaternion newRotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSmoothing * Time.deltaTime);
		// Change the players rotation to this new rotation.
		transform.rotation = newRotation;
	}

	void StopTurning() {
		anim.SetFloat(TurnAngFloat,0);
		anim.SetBool(TurnBool,false);
		holdTurnCalc = false;
		//Debug.Log("TurnStop");
	}
}