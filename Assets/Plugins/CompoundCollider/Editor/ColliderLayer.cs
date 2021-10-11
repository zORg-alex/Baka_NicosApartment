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
		public Dictionary<int, Triangle> triangles;

		/// <summary>
		/// Used to draw in editor
		/// </summary>
		public Mesh mesh;
		public zCollider Parent;
		public string name;

		public Color color;

		///// <summary>
		///// collider on zCollider that should have newly generated mesh on
		///// </summary>
		//public MeshCollider collider;

		//[SerializeField, SerializeReference]
		//public zColliderPainterScript Painter;



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

		private void UpdateMesh() {
			var verts = triangles.Values.SelectMany(t => t).Distinct().ToArray();
			var tris = triangles.SelectMany(t => new int[] { ArrayUtility.IndexOf(verts, t.Value[0]), ArrayUtility.IndexOf(verts, t.Value[1]), ArrayUtility.IndexOf(verts, t.Value[2]) }).ToArray();

			if (mesh == null)
				mesh = new Mesh();
			else
				mesh.Clear();
			mesh.name = name;
			mesh.vertices = verts;
			mesh.triangles = tris;
			mesh.RecalculateNormals();
		}

		public void Paint() {
			zColliderPainterEditorWindow.Paint(this);
		}

		public override bool Equals(object obj) {
			return obj is ColliderLayer layer &&
				   EqualityComparer<Dictionary<int, Triangle>>.Default.Equals(triangles, layer.triangles) &&
				   EqualityComparer<Mesh>.Default.Equals(mesh, layer.mesh) &&
				   EqualityComparer<zCollider>.Default.Equals(Parent, layer.Parent) &&
				   name == layer.name &&
				   color.Equals(layer.color);
		}

		public override int GetHashCode() {
			int hashCode = 2097467385;
			hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<int, Triangle>>.Default.GetHashCode(triangles);
			hashCode = hashCode * -1521134295 + EqualityComparer<Mesh>.Default.GetHashCode(mesh);
			hashCode = hashCode * -1521134295 + EqualityComparer<zCollider>.Default.GetHashCode(Parent);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
			hashCode = hashCode * -1521134295 + color.GetHashCode();
			return hashCode;
		}

		public bool ShowGizmo;

		public ColliderLayer(Color color, string name) {
			this.color = color;
			this.name = name;
			triangles = new Dictionary<int, Triangle>();
			mesh = null;
			Parent = zColliderPainterEditorWindow.currentColliderScript;
			//Painter = zColliderPainterEditorWindow.currentPainter;
			ShowGizmo = false;
		}

		public ColliderLayer(zCollider zCol, int index) {
			color = zCol.layersColors[index];
			name = zCol.layerMeshNames[index];
			mesh = zCol.GetRawMesh(index);
			Parent = zCol;
			triangles = zCol.layerMeshTriangles[index].ToDictionary(t=>t.GetHashCode(), t=>t);
			Parent = zColliderPainterEditorWindow.currentColliderScript;
			//Painter = zColliderPainterEditorWindow.currentPainter;
			ShowGizmo = false;
			UpdateMesh();
		}


	}
}