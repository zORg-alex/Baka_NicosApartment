using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	[RequireComponent(typeof(MeshFilter))]
	[ExecuteInEditMode]
	public partial class CompoundColliderSettingsScript : MonoBehaviour {

		public MeshCollider internalCollider;
		[SerializeField]
		private Vector3[] internalColliderVertices;
		[SerializeField]
		private int[] internalColliderTriangles;
		public List<ColliderSettings> stack = new List<ColliderSettings>();
		private new Rigidbody rigidbody;

		private void OnEnable() {
			//TODO Destroy self on play?

			var mesh = GetComponent<MeshFilter>()?.sharedMesh;
			var meshCollider = GetComponents<MeshCollider>().FirstOrDefault(c => c.sharedMesh == mesh);
			if (meshCollider != null) {
				Utils.Destroy(meshCollider);
			}
			foreach (var s in stack) {
				s.Script = this;
			}

			internalCollider = gameObject.AddComponent<MeshCollider>();
			internalCollider.convex = false;
			internalColliderVertices = internalCollider.sharedMesh.vertices;
			internalColliderTriangles = internalCollider.sharedMesh.triangles;                                             

			rigidbody = GetComponent<Rigidbody>();
		}

		internal void TryRestore() {
			var colliders = GetComponents<MeshCollider>().Where(c => c != internalCollider);
			if (stack.Count == 0 && colliders.Count() > 0) {
				foreach (var c in colliders) {
					var s = new ColliderSettings(Color.HSVToRGB(UnityEngine.Random.value, 1, 1), c.sharedMesh.name);
					s.collider = c;
					s.mesh = c.sharedMesh;
					s.RestoreTriangles();
					s.ShowGizmo = true;
					stack.Add(s);
					s.Script = this;
				}
			}

		}

		private void OnDisable() {
			Utils.Destroy(internalCollider);
		}

		private ColliderSettings paintedCollider;

		[SerializeField]
		[HideInInspector]
		internal bool isPainting;

		internal void RemoveFromStack(IEnumerable<ColliderSettings> removedSettings) {
			foreach (var rs in removedSettings) {
				Utils.Destroy(rs.collider);
				stack.Remove(rs);
			};
		}

		internal void StartPainting(ColliderSettings colliderSettings) {
			paintedCollider = colliderSettings;
			isPainting = true;
		}
		internal void StopPainting() {
			if (isPainting) Debug.Log("Stopped Painting");
			isPainting = false;
		}

		public void PaintTriangle(int VertexIndex, bool subtract = false) {
			if (VertexIndex == -1) return;
			try {
				var tri = new Vector3[] {
				internalColliderVertices[ internalColliderTriangles[VertexIndex * 3]],
				internalColliderVertices[ internalColliderTriangles[VertexIndex * 3 + 1]],
				internalColliderVertices[ internalColliderTriangles[VertexIndex * 3 + 2]]
			};
				if (!subtract)
					paintedCollider.AddTriangle(tri);
				else
					paintedCollider.RemoveTriangle(tri);
			} catch (Exception) { }
		}

		internal ColliderSettings NewCollider(string name) {
			var coll = new ColliderSettings(Color.HSVToRGB(UnityEngine.Random.value, 1f, 1f), name);
			coll.Script = this;
			stack.Add(coll);
			return coll;
		}

		[Button]
		public void CleanUp() {
			Editor?.Clean();
			Clean();
		}

		internal void Clean() {
			Utils.Destroy(internalCollider);
			Utils.Destroy(this);
		}

		internal void GenerateColliders() {
			foreach (var c in stack) {
				if (c.collider == null) {
					c.collider = gameObject.AddComponent<MeshCollider>();
				}
				c.collider.convex = true;
				c.mesh.name = c.name;
				c.collider.sharedMesh = c.mesh;

			}
		}

		public CompoundColliderEditor Editor { get; internal set; }
		public int? PaintedTri { get; internal set; }

		private void OnDrawGizmosSelected() {
			foreach (var c in stack) {
				if (c.mesh != null && c.ShowGizmo)
					if (c.mesh.triangles.Count() > 0) {
						Gizmos.color = c.color;
						Gizmos.matrix = transform.localToWorldMatrix;
						Gizmos.DrawMesh(c.mesh);
					}
			}
		}
	}
}