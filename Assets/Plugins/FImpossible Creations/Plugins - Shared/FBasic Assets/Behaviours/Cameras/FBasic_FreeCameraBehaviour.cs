using UnityEngine;
using UnityEngine.InputSystem;

namespace FIMSpace.Basics
{
    /// <summary>
    /// FM: Simple component to controll behaviour of camera in free flying mode
    /// </summary>
    public class FBasic_FreeCameraBehaviour : MonoBehaviour {
        public InputAction mb1;
        public InputAction kbSpace;
        public InputAction mouseDelta;
        public InputAction wasd;
        public InputAction shift;
        public InputAction ctrl;

        [Header("> Hold right mouse button to rotate camera <")]
        [Tooltip("How fast camera should fly")]
        public float SpeedMultiplier = 10f;
        [Tooltip("Value of acceleration smoothness")]
        public float AccelerationSmothnessValue = 10f;

        [Tooltip("Value of rotation smoothness")]
        public float RotationSmothnessValue = 10f;

        /// <summary> Just multiplier for rotation </summary>
        public float MouseSensitivity = .2f;

        public bool NeedRMB = true;

        /// <summary> Variables controlling turbo speed on shift key </summary>
        private float turboModeMultiply = 5f;

        /// <summary> Variable to hold speeds of directions for camera to fly </summary>
        private Vector3 speeds;

        private float ySpeed;

        /// <summary> Holding rotation value for camera to rotate</summary>
        private Vector3 rotation;

        /// <summary> Turbo multiplier for faster movement </summary>
        private float turbo = 1f;

        /// <summary> 
        /// Just initializing few variables 
        /// </summary>
        void OnEnable()
        {
            mb1 = new InputAction("mb0", InputActionType.Button, "<Mouse>/rightButton", "press(behavior=1)");
            mouseDelta = new InputAction("mDel", InputActionType.Value, "<Mouse>/delta");
            kbSpace = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
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
            kbSpace.Enable();
            wasd.Enable();
            shift.Enable();
            ctrl.Enable();
            speeds = Vector3.zero;
            ySpeed = 0f;
            rotation = transform.rotation.eulerAngles;
        }

        void Update()
        {

            // Detecting key movement factors
            float f = wasd.ReadVector2().y;
            float r = wasd.ReadVector2().x;

            float forward = f * Time.smoothDeltaTime * SpeedMultiplier;
            float right = r * Time.smoothDeltaTime * SpeedMultiplier;

            // Smooth change turbo speed
            if (shift.ReadBool())
            {
                turbo = Mathf.Lerp(turbo, turboModeMultiply, Time.smoothDeltaTime * 5f);
            }
            else
            {
                turbo = Mathf.Lerp(turbo, 1f, Time.smoothDeltaTime * 5f);
            }

            forward *= turbo;
            right *= turbo;

            if (Cursor.lockState != CursorLockMode.None)
            {
                // Rotation with pressed rmb
                if (mb1.ReadBool() || !NeedRMB )
                {
                    var md = mouseDelta.ReadVector2();
                    rotation.x -= (md.y * 1f * MouseSensitivity);
                    rotation.y += (md.x * 1f * MouseSensitivity);
                }
            }

            // Lerping speed variables for smooth effect
            speeds.z = Mathf.Lerp(speeds.z, forward, Time.smoothDeltaTime * AccelerationSmothnessValue);
            speeds.x = Mathf.Lerp(speeds.x, right, Time.smoothDeltaTime * AccelerationSmothnessValue);

            // Applying translation for current transform orientation
            transform.position += transform.forward * speeds.z;
            transform.position += transform.right * speeds.x;
            transform.position += transform.up * speeds.y;

            // Lerping rotation for smooth effect
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), Time.smoothDeltaTime * RotationSmothnessValue);

            // Going in Up / Down directions in world reference
            if (ctrl.ReadBool())
            {
                ySpeed = Mathf.Lerp(ySpeed, 1f, Time.smoothDeltaTime * AccelerationSmothnessValue);
            }
            else
            if (kbSpace.ReadBool())
            {
                ySpeed = Mathf.Lerp(ySpeed, -1f, Time.smoothDeltaTime * AccelerationSmothnessValue);
            }
            else
                ySpeed = Mathf.Lerp(ySpeed, 0f, Time.smoothDeltaTime * AccelerationSmothnessValue);

            transform.position += Vector3.down * ySpeed * turbo * Time.smoothDeltaTime * SpeedMultiplier;
        }

        /// <summary> 
        /// Cursor locking stuff 
        /// </summary>
        public void FixedUpdate()
        {
            if (mb1.ReadBool())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                if (NeedRMB)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
    }
}