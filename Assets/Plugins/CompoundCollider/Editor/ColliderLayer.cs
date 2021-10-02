using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	/// <summary>
	/// Layer for painting, stores selected meshe's
	/// </summary>
	[Serializable]
	public class ColliderLayer {
		/// <summary>
		/// Dictionary of triangle structs to generate mesh from
		/// </summary>
		[SerializeField]
		public Dictionary<int, Triangle> triangles = new Dictionary<int, Triangle>();

		public Mesh mesh;

		public void AddTriangle(int meshIndex, Vector3[] verts) {
			var oldTrisCount = triangles.Count;
			var triangle = new Triangle(meshIndex, verts);
			triangles[triangle.GetHashCode()] = triangle;
			Debug.Log($"Added {triangle} {triangle.GetHashCode()}");

			if (oldTrisCount != triangles.Count)
				UpdateMesh();
		}
		internal void RemoveTriangle(int meshIndex, Vector3[] verts) {
			var oldTrisCount = triangles.Count;
			var triangle = new Triangle(meshIndex, verts);
			triangles.Remove(triangle.GetHashCode());
			if (triangles.Count != oldTrisCount)
				UpdateMesh();
		}


		public void RestoreTriangles() {
			//TODO
			//triangles.Clear();
			//for (int i = 0; i < mesh.triangles.Count() / 3; i++) {
			//	var tri = new Triangle(new Vector3[] { mesh.vertices[mesh.triangles[i * 3]], mesh.vertices[mesh.triangles[i * 3 + 1]], mesh.vertices[mesh.triangles[i * 3 + 2]] });
			//	triangles[tri.GetHashCode()] = tri;
			//}
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
		public string name;

		public Color color = Color.white;

		public void Paint() {
			Debug.Log("Painting started");
			Painter.StopPainting();
			Painter.StartPainting(this);
		}

		public bool ShowGizmo = true;

		public ColliderLayer(Color color, string name) {
			this.color = color;
			this.name = name;
		}

		/// <summary>
		/// collider on zCollider that should have newly generated mesh on
		/// </summary>
		public MeshCollider collider;

		[SerializeField, SerializeReference]
		public zColliderPainterScript Painter;

	}
}