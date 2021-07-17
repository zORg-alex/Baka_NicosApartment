using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	[Serializable]
	public class ColliderSettings {
		[SerializeField]
		public Dictionary<int, Triangle> triangles = new Dictionary<int, Triangle>();

		public Mesh mesh;

		public void AddTriangle(Vector3[] verts) {
			var oldTrisCount = triangles.Count;
			var triangle = new Triangle(verts);
			triangles[triangle.GetHashCode()] = triangle;
			Debug.Log($"Added {triangle} {triangle.GetHashCode()}");

			if (oldTrisCount != triangles.Count)
				UpdateMesh();
		}

		public void RestoreTriangles() {
			triangles.Clear();
			for (int i = 0; i < mesh.triangles.Count() / 3; i++) {
				var tri = new Triangle(new Vector3[] { mesh.vertices[mesh.triangles[i * 3]], mesh.vertices[mesh.triangles[i * 3 + 1]], mesh.vertices[mesh.triangles[i * 3 + 2]] });
				triangles[tri.GetHashCode()] = tri;
			}
		}

		private void UpdateMesh() {
			var verts = triangles.Values.SelectMany(t => t).Distinct().ToArray();
			var tris = triangles.SelectMany(t => new int[] { ArrayUtility.IndexOf(verts, t.Value[0]), ArrayUtility.IndexOf(verts, t.Value[1]), ArrayUtility.IndexOf(verts, t.Value[2]) }).ToArray();

			mesh = new Mesh();
			mesh.name = name;
			mesh.vertices = verts;
			mesh.triangles = tris;
			mesh.RecalculateNormals();
		}
		internal void RemoveTriangle(Vector3[] verts) {
			var oldTrisCount = triangles.Count;
			var triangle = new Triangle(verts);
			triangles.Remove(triangle.GetHashCode());
			if (triangles.Count != oldTrisCount)
				UpdateMesh();
		}

		public string name;

		public Color color = Color.white;

		public void Paint() {
			Debug.Log("Painting started");
			Script.StopPainting();
			Script.StartPainting(this);
		}

		public bool ShowGizmo = true;

		public ColliderSettings(Color color, string name) {
			this.color = color;
			this.name = name;
		}

		public MeshCollider collider;

		[SerializeField, SerializeReference]
		public CompoundColliderSettingsScript Script;

	}
}