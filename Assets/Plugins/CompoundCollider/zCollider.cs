using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace ZorgsCompoundColliders {
	public class zCollider : MonoBehaviour {
		public Mesh[] meshes;
		public Collider[] colliders;
		public Vector3[][] meshesVertices;
		public int[][] meshesPaintedTriangles;
		public string[] meshNames;
		public Color[] colliderColors;

		[SerializeField, HideInInspector]
		private bool initialized;
		public MeshCollider[] generatedColliders = new MeshCollider[0];

		private void OnEnable() {
			//Check if all colliders are ok
			if (!initialized) {
				initialized= true;
			} else {
				RegenerateColliders();
			}
		}

		public void RegenerateColliders(bool force = false) {
			var meshcolliders = GetComponents<MeshCollider>();
			if (meshcolliders.Length < meshes.Length) {
				//Add more mesh colliders
				for (int i = 0; i < meshes.Length - meshcolliders.Length; i++) {
					gameObject.AddComponent<MeshCollider>();
				}
			} else if (meshcolliders.Length > meshes.Length) {
				//remove unnecessary meshcolliders
			}
			int counter = 0;
			foreach (var mc in meshcolliders) {
				//Check name and triangle count and replace if necessary
				//Optimization should ruin all vertices and triangles to check for, I guess, so nothing to compare to, except count
				if (mc.sharedMesh == null || force || mc.name != meshNames[counter] || mc.sharedMesh.triangles.Count() != meshesPaintedTriangles[counter].Count()) {
					mc.name = meshNames[counter];
					var mesh = mc.sharedMesh == null ? new Mesh() : mc.sharedMesh;
					mesh.Clear();
					mesh.vertices = meshesVertices[counter];
					mesh.triangles = meshesPaintedTriangles[counter];
					mesh.Optimize();
					mc.sharedMesh = mesh;
				}
				counter++;
			}
		}

#if UNITY_EDITOR
		public List<ColliderLayer> Layers = new List<ColliderLayer>();


#endif
	}
#if UNITY_EDITOR
	[CustomEditor(typeof(zCollider))]
	public class zColliderEditor : Editor {
		/// <summary>
		/// Show big Clear button if there are nothing painted
		/// </summary>
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
		}

		private int pathEditorHash;
		private zCollider obj;

		void OnEnable() {
			Debug.Log($"OnEnable");
			pathEditorHash = this.GetHashCode();
			obj = serializedObject.targetObject as zCollider;
		}
		void OnSceneGUI() {
			Event current = Event.current;
			int controlID = GUIUtility.GetControlID(pathEditorHash, FocusType.Passive);

			// If we are in edit mode and the user clicks (right click, middle click or alt+left click)
			if (Application.isEditor && zColliderPainterEditorWindow.IsPainting) {

				if (current.type == EventType.Layout)
					//Magic thing
					HandleUtility.AddDefaultControl(controlID);
				else if ((current.type == EventType.MouseDrag || current.type == EventType.MouseDown) && current.button == 0) {
					Ray worldRay = Camera.current.ScreenPointToRay(new Vector3(current.mousePosition.x, Screen.height - current.mousePosition.y - 36, 0));
					var hits = Physics.RaycastAll(worldRay, float.PositiveInfinity);
					var hit = hits.FirstOrDefault(h => obj.colliders.Contains(h.collider));
					if (hit.collider != null) {
						zColliderPainterEditorWindow.PaintTriangle(hit.collider, hit.triangleIndex);
					}
					current.Use();
				} else if ((current.type == EventType.MouseDown && current.button == 1) || (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)) {
					zColliderPainterEditorWindow.Paint();
					current.Use();
				} else if (current.type == EventType.KeyDown && current.keyCode == KeyCode.LeftControl)
					zColliderPainterEditorWindow.SubtractiveMode = true;
				else if (current.type == EventType.KeyUp && current.keyCode == KeyCode.LeftControl)
					zColliderPainterEditorWindow.SubtractiveMode = false;

				if (zColliderPainterEditorWindow.SubtractiveMode)
					EditorGUIUtility.AddCursorRect(SceneView.lastActiveSceneView.position, MouseCursor.ArrowMinus);
				else
					EditorGUIUtility.AddCursorRect(SceneView.lastActiveSceneView.position, MouseCursor.ArrowPlus);
			}
		}
	}
#endif
}