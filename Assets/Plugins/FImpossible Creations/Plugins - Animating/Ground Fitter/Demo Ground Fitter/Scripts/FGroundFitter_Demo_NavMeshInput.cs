using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace FIMSpace.GroundFitter
{
    public class FGroundFitter_Demo_NavMeshInput : MonoBehaviour
    {
        public NavMeshAgent TargetAgent;
		public InputAction mb0;
        public InputAction mousePos;

		private void OnEnable() {
            mb0 = new InputAction("RightMouseButton", InputActionType.Value, "<Mouse>/rightButton", "press(behavior=1)");
            mb0.Enable();
            mousePos = new InputAction("M pos", InputActionType.Value, "<Mouse>/position");
            mousePos.Enable();
        }

        void Update()
        {
            if (mb0.ReadValue<float>() > 0)
            {
                if (TargetAgent)
                {
                    Ray ray = Camera.main.ScreenPointToRay(mousePos.ReadVector2());

                    RaycastHit hit;
                    if (Physics.Raycast(ray.origin, ray.direction, out hit))
                    {
                        NavMeshHit navMeshHit;
                        if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1, 1))
                        {
                            TargetAgent.SetDestination(navMeshHit.position);
                        }
                    }
                }
            }
        }
    }
}