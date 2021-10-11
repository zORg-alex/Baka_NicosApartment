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
		/// colliders to be raypicked for triangles, from objects meshes
		/// </summary>
		public MeshCollider[] colliders;
		/// <summary>
		/// copy of meshes vertices for saving picked triangles
		/// </summary>
		private Vector3[][] collidersVertices;
		/// <summary>
		/// Copy of meshes triangles vertex indexes 
		/// </summary>
		private int[][] collidersTriangles;
		/// <summary>
		/// Currently selected object with zCollider
		/// </summary>
		public zCollider currentZCollider;

		[NonReorderable]
		public List<ColliderLayer> layers = new List<ColliderLayer>();
		public new Rigidbody rigidbody;

		/// <summary>
		/// Editor hash
		/// </summary>
		internal int pathEditorHash;

		private void OnEnable() {
			//TODO Destroy self on play?

			zColliderChanged();

			pathEditorHash = this.GetHashCode();
		}

		internal void zColliderChanged() {
			if (currentZCollider) {
				var meshes = currentZCollider.GetComponentsInChildren<MeshFilter>();
				Utils.Destroy(colliders);
				colliders = new MeshCollider[meshes.Count()];
				collidersVertices = new Vector3[meshes.Count()][];
				collidersTriangles = new int[meshes.Count()][];
				int counter = 0;
				foreach (MeshFilter meshfilter in meshes) {
					colliders[counter] = gameObject.AddComponent<MeshCollider>();
					if (meshfilter.sharedMesh != null) {
						//Apply child object transforms to vertices
						collidersVertices[counter] = meshfilter.sharedMesh.vertices.Select(v=>transform.InverseTransformPoint(meshfilter.transform.TransformPoint(v))).ToArray();
						collidersTriangles[counter] = meshfilter.sharedMesh.triangles;
					}
					var mesh = new Mesh();
					mesh.vertices = collidersVertices[counter];
					mesh.triangles = collidersTriangles[counter];
					mesh.name = meshfilter.name;
					colliders[counter].sharedMesh = mesh;
					colliders[counter].convex = false;
					counter++;
				}
				layers.Clear();
				for (int i = 0; i < currentZCollider.layerMeshNames?.Length; i++) {
					layers.Add(new ColliderLayer(currentZCollider, i));
				}
			} else {
				layers.Clear();
			}
		}

		internal void SelectionChangedTo(zCollider zCollider) {
			if (zCollider) {
				transform.position = zCollider.transform.position;
				transform.rotation = zCollider.transform.rotation;
				transform.localScale = zCollider.transform.localScale;
			}
			currentZCollider = zCollider;
			zColliderChanged();
		}

		private void OnDisable() {
			//Utils.Destroy(colliders);
			foreach (var c in colliders) {
				if (c)
					Utils.Destroy(c);
			}
		}

		internal ColliderLayer paintedColliderLayer;

		[SerializeField]
		[HideInInspector]
		internal bool isPainting;

		internal void PaintWithRay(Ray ray, ColliderLayer layer, bool subtractiveMode) {
			foreach (var col in colliders) {
				if (col.Raycast(ray, out var hit, float.PositiveInfinity)) {
					PaintTriangle(col, hit.triangleIndex, subtractiveMode);
				}
			}
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
					paintedColliderLayer.AddTriangle(meshIndex, tri);
				else
					paintedColliderLayer.RemoveTriangle(meshIndex, tri);
			} catch (Exception) { }
		}

		internal ColliderLayer AddColliderLayer(string name) {
			var layer = new ColliderLayer(Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), name) {
				//Painter = this,
				Parent = currentZCollider
			};
			layers.Add(layer);
			if (paintedColliderLayer == null)
				paintedColliderLayer = layer;
			return layer;
		}

		internal void RemoveColliderLayer(ColliderLayer layer) {
			if (layer == default) return;
			layers.Remove(layer);
			//Utils.Destroy(layer.collider);
		}


		internal void Clean() {
			Utils.Destroy(colliders);
			currentZCollider = null;
			colliders = null;
			layers.Clear();
		}

		internal void SetDrawGizmos(bool value) {
			_drawGizmos = value;
		}
		private bool _drawGizmos;
		internal void OnDrawGizmos() {
			if (!_drawGizmos) return;
			bool HighlightCurrent = zColliderPainterEditorWindow.IsPainting && zColliderPainterEditorWindow.currentLayer != null;
			foreach (var c in layers) {
				if (c.mesh != null && c.ShowGizmo)
					if (c.mesh.triangles.Count() > 0) {
						if (HighlightCurrent && c != zColliderPainterEditorWindow.currentLayer)
							Gizmos.color = c.color.MultiplyAlpha(.7f);
						else
							Gizmos.color = c.color;
						Gizmos.matrix = transform.localToWorldMatrix;
						Gizmos.DrawMesh(c.mesh);
					}
			}
		}

		internal void ApplyColliderChanges() {
			currentZCollider.IsNull()?.UpdateTriangles(layers);
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
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
		}
	}
}