using FIMSpace.GroundFitter;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FIMSpace.Basics
{
    public class FBasics_RigidbodyMover : FBasics_RigidbodyMoverBase
    {
        public float MovementSpeed = 4f;
        public float RotationSpeed = 10f;
        public float JumpPower = 7f;
        [Tooltip("Use keyboard keys movement implementation for quick debugging?")]
        public bool WSADMovement = true;

        public InputAction mb1;
        public InputAction space;
        public InputAction mouseDelta;
        public InputAction wasd;
        public InputAction shift;
        public InputAction ctrl;

        private void OnEnable() {
            mb1 = new InputAction("mb0", InputActionType.Button, "<Mouse>/rightButton", "press(behavior=1)");
            mouseDelta = new InputAction("mDel", InputActionType.Value, "<Mouse>/delta");
            space = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            wasd = new InputAction("WASD");
            wasd.AddCompositeBinding("2DVector", "press(behavior=1)") // Or "Dpad"
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            shift = new InputAction("Shift", InputActionType.Button, "<Keyboard>/leftShift", "press(behavior=1)");
            ctrl = new InputAction("Ctrl", InputActionType.Button, "<Keyboard>/leftCtrl", "press(behavior=1)");
            mb1.Enable();
            mouseDelta.Enable();
            space.Enable();
            wasd.Enable();
            shift.Enable();
            ctrl.Enable();
        }

        [Range(0f, 0.5f)]
        [Tooltip("How slow accelerate/decelerate should be")]
        public float accelerationTime = 0.1f;
        [Tooltip("Always rotate head towards movement direction")]
        public bool RotateInDir = true;

        private float offsetRotY = 0f;
        private Vector3 moveDir = Vector3.zero;
        private Vector3 targetRot;

        protected override void Start()
        {
            base.Start();

            targetRot = transform.rotation.eulerAngles;
        }

        protected virtual void Update()
        {
            UpdateMotor();
        }


        protected override void UpdateMotor()
        {
            moveSpeed = MovementSpeed;

            moveDir = Vector3.zero;
            offsetRotY = 0f;


            if (WSADMovement)
            {
                moveDir = wasd.ReadVector2();

                // Debug sprint
                moveSpeed *= 1.5f * shift.ReadFloat();


                // Triggering jump to be executed in next fixed update
                if (isGrounded) if (space.ReadBool()) triggerJumping = JumpPower;
            }


            // Defining rotation for object
            if (moveDir != Vector3.zero)
            {
                moveDir.Normalize();

                if (RotateInDir)
                {
                    targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.TransformDirection(moveDir), Vector3.up)).eulerAngles;
                    moveDir = Vector3.forward;
                }
                else
                    targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up)).eulerAngles;

                targetRot.y += offsetRotY;
            }


            // Calculating smooth acceleration value to be used in next fixed update frame
            smoothedAcceleration = Vector3.SmoothDamp(smoothedAcceleration, moveDir, ref veloHelper, accelerationTime, Mathf.Infinity, Time.deltaTime);

            // Calculating smooth rotation to be applied in fixes update
            smoothedRotation = Quaternion.Lerp(rigbody.rotation, Quaternion.Euler(targetRot), Time.deltaTime * RotationSpeed);
        }

    }
}