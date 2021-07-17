using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
    [CustomEditor(typeof(CompoundColliderSettingsScript))]
    public class CompoundColliderSettingsScriptInspector : OdinEditor {
        private SerializedProperty isPainting;
        private int pathEditorHash;
        private bool subtractiveMode;

        public CompoundColliderSettingsScript obj { get; private set; }

        new void OnEnable() {
            base.OnEnable();
            // Setup the SerializedProperties.
            isPainting = serializedObject.FindProperty("isPainting");
            Debug.Log($"OnEnable");
            pathEditorHash = this.GetHashCode();
            obj = serializedObject.targetObject as CompoundColliderSettingsScript;
        }

        void OnSceneGUI() {
            Event current = Event.current;
            int controlID = GUIUtility.GetControlID(pathEditorHash, FocusType.Passive);

            // If we are in edit mode and the user clicks (right click, middle click or alt+left click)
            if (Application.isEditor && isPainting != null && isPainting.boolValue) {

                if (current.type == EventType.Layout)
                    //Magic thing
                    HandleUtility.AddDefaultControl(controlID);
                else if ((current.type == EventType.MouseDrag || current.type == EventType.MouseDown) && current.button == 0) {
                    Ray worldRay = Camera.current.ScreenPointToRay(new Vector3(current.mousePosition.x, Screen.height - current.mousePosition.y - 36, 0));
                    RaycastHit hitInfo;
                    var hit = obj.internalCollider.Raycast(worldRay, out hitInfo, 10000);
                    if (hit) {
                        if (!subtractiveMode)
                            obj.PaintTriangle(hitInfo.triangleIndex);
                        else
                            obj.PaintTriangle(hitInfo.triangleIndex, true);
                    }
                    current.Use();
                } else if ((current.type == EventType.MouseDown && current.button == 1) || (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)) {
                    obj.StopPainting();
                    current.Use();
                } else if (current.type == EventType.KeyDown && current.keyCode == KeyCode.LeftControl)
                    subtractiveMode = true;
                else if (current.type == EventType.KeyUp && current.keyCode == KeyCode.LeftControl)
                    subtractiveMode = false;

                if (subtractiveMode)
                    EditorGUIUtility.AddCursorRect(SceneView.lastActiveSceneView.position, MouseCursor.ArrowMinus);
                else
                    EditorGUIUtility.AddCursorRect(SceneView.lastActiveSceneView.position, MouseCursor.ArrowPlus);
            }
        }
    }
}