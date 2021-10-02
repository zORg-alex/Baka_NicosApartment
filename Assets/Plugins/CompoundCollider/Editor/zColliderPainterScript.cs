using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	/// <summary>
	/// Adds colliders for painting from selected objects meshes
	/// receives paint and UI input, from This script's editor and master script, shoud change to have only Scene UI thing instead of window
	/// </summary>
	[RequireComponent(typeof(MeshFilter))]
	[ExecuteInEditMode]
	public partial class zColliderPainterScript : MonoBehaviour {

		/// <summary>
		/// colliders to be raypicked for triangles
		/// </summary>
		public MeshCollider[] colliders;
		/// <summary>
		/// Currently selected object with zCollider
		/// </summary>
		public zCollider currentZCollider;
		/// <summary>
		/// copy of meshes vertices for saving picked triangles
		/// </summary>
		private Vector3[][] collidersVertices;
		/// <summary>
		/// Copy of meshes triangles vertex indexes 
		/// </summary>
		private int[][] collidersTriangles;
		[NonReorderable]
		public List<ColliderLayer> layers = new List<ColliderLayer>();
		public new Rigidbody rigidbody;

		private void OnEnable() {
			//TODO Destroy self on play?

			if (currentZCollider != null) {
				var meshes = currentZCollider.GetComponentsInChildren<MeshFilter>();
				colliders = new MeshCollider[meshes.Count()];
				collidersVertices = new Vector3[meshes.Count()][];
				collidersTriangles = new int[meshes.Count()][];
				int counter = 0;
				foreach (MeshFilter mesh in meshes) {
					colliders[counter] = gameObject.AddComponent<MeshCollider>();
					colliders[counter].sharedMesh = mesh.sharedMesh;
					colliders[counter].convex = false;
					collidersVertices[counter] = colliders[counter].sharedMesh.vertices;
					collidersTriangles[counter] = colliders[counter].sharedMesh.triangles;
					counter++;
				}
			}
		}

		internal void SetEditor(zColliderEditor zColliderEditor) {
			Editor = zColliderEditor;
		}

		internal void SelectionChanged(zCollider zCollider) {
			currentZCollider = zCollider;

		}

		internal void TryRestore() {
			//TODO new approach will have different solution... Way easier one
			if (currentZCollider != null) {
				var zCollider = currentZCollider.GetComponent<zCollider>();
			}
		}

		private void OnDisable() {
			//Utils.Destroy(colliders);
			foreach (var c in colliders) {
				Utils.Destroy(c);
			}
		}

		private ColliderLayer paintedCollider;

		[SerializeField]
		[HideInInspector]
		internal bool isPainting;

		internal void StartPainting(ColliderLayer colliderSettings) {
			paintedCollider = colliderSettings;
			isPainting = true;
		}
		internal void StopPainting() {
			if (isPainting) Debug.Log("Stopped Painting");
			isPainting = false;
		}

		internal void PaintTriangle(Collider collider, int triangleIndex, bool subtract = false) => PaintTriangle(ArrayUtility.IndexOf(colliders, collider), triangleIndex, subtract);
		internal void PaintTriangle(int meshIndex, int VertexIndex, bool subtract = false) {
			if (VertexIndex == -1) return;
			try {
				Vector3[] verts = collidersVertices[meshIndex];
				int[] tris = collidersTriangles[meshIndex];
				var tri = new Vector3[] {
					verts[tris[VertexIndex * 3]],
					verts[tris[VertexIndex * 3 + 1]],
					verts[tris[VertexIndex * 3 + 2]]
				};
				if (!subtract)
					paintedCollider.AddTriangle(meshIndex, tri);
				else
					paintedCollider.RemoveTriangle(meshIndex, tri);
			} catch (Exception) { }
		}

		internal ColliderLayer AddColliderLayer(string name) {
			var coll = new ColliderLayer(Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), name);
			coll.Painter = this;
			layers.Add(coll);
			return coll;
		}

		internal void RemoveColliderLayer(IEnumerable<ColliderLayer> removedLayers) {
			foreach (var rs in removedLayers) {
				layers.Remove(rs);
				Utils.Destroy(rs.collider);
			};
		}


		internal void Clean() {
			Utils.Destroy(colliders);
			Utils.Destroy(this);
		}

		internal void GenerateColliders() {
			foreach (var l in layers) {
				if (l.collider == null) {
					l.collider = gameObject.AddComponent<MeshCollider>();
				}
				l.mesh.name = l.name;
				l.collider.sharedMesh = l.mesh;
				l.collider.convex = true;

			}
		}

		public zColliderEditor Editor { get; internal set; }
		public int? PaintedTri { get; internal set; }
		public bool HasRigidBody => rigidbody != null;

		private void OnDrawGizmosSelected() {
			foreach (var c in layers) {
				if (c.mesh != null && c.ShowGizmo)
					if (c.mesh.triangles.Count() > 0) {
						Gizmos.color = c.color;
						Gizmos.matrix = transform.localToWorldMatrix;
						Gizmos.DrawMesh(c.mesh);
					}
			}
		}

	}

	/// <summary>
	/// On mouse input raypick triangles on painter object
	/// 
	/// </summary>
	[CustomEditor(typeof(zColliderPainterScript))]
	public class zColliderPainterScriptEditor : Editor {
		private SerializedProperty isPainting;
		private int pathEditorHash;
		private bool subtractiveMode;

		public zColliderPainterScript obj { get; private set; }

		void OnEnable() {
			// Setup the SerializedProperties.
			isPainting = serializedObject.FindProperty("isPainting");
			Debug.Log($"OnEnable");
			pathEditorHash = this.GetHashCode();
			obj = serializedObject.targetObject as zColliderPainterScript;
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
					var hits = Physics.RaycastAll(worldRay, float.PositiveInfinity);
					var hit = hits.FirstOrDefault(h => obj.colliders.Contains(h.collider));
					if (hit.collider != null) {
						if (!subtractiveMode)
							obj.PaintTriangle(hit.collider, hit.triangleIndex);
						else
							obj.PaintTriangle(hit.collider, hit.triangleIndex, true);
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