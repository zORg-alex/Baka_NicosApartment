using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {

	[ExecuteInEditMode]
	public class zCollider : MonoBehaviour {
		/// <summary>
		/// Original meshes of this object to be painted
		/// </summary>
		public Mesh[] meshes;
		/// <summary>
		/// Links to scripts final colliders from layers
		/// </summary>
		public MeshCollider[] layersColliders;
		/// <summary>
		/// Final colliders triangles == layer triangles
		/// </summary>
		public ArrayElement<Triangle>[] layerMeshTriangles;
		/// <summary>
		/// Final colliders triangle vertices (unoptimaized)
		/// </summary>
		public ArrayElement<Vector3>[] layerMeshVertices;
		/// <summary>
		/// Final colliders triangle vertices indexes
		/// </summary>
		public ArrayElement<int>[] layerMeshPaintedTriangles;
		/// <summary>
		/// Final collider mesh names == layer names
		/// </summary>
		public string[] layerMeshNames;

		[SerializeField, HideInInspector]
		private bool initialized;
		public MeshCollider[] generatedColliders = new MeshCollider[0];

		private void OnEnable() {
			//Check if all colliders are ok
			if (!initialized) {
				meshes = GetComponentsInChildren<MeshFilter>().Select(mf => mf.sharedMesh).ToArray();
				initialized= true;
			} else {
				RegenerateColliders();
			}
		}

		/// <summary>
		/// (Re)Generate colliders from data ini this script
		/// </summary>
		/// <param name="force"></param>
		public void RegenerateColliders(bool force = false) {
			layersColliders = GetComponents<MeshCollider>();
			if (layersColliders.Length < layerMeshNames.Length) {
				//Add more mesh colliders
				for (int i = layersColliders.Length; i < layerMeshNames.Length; i++) {
					var mc = gameObject.AddComponent<MeshCollider>();
				}
			} else if (layersColliders.Length > layerMeshNames.Length) {
				for (int i = layerMeshNames.Length; i < layersColliders.Length; i++) {
					Utils.Destroy(layersColliders[i]);
				}
			}
			layersColliders = GetComponents<MeshCollider>();
			int counter = 0;
			foreach (var mc in layersColliders) {
				//Check name and triangle count and replace if necessary
				//Optimization should ruin all vertices and triangles to check for, I guess, so nothing to compare to, except count
				if (mc.sharedMesh == null || force || mc.sharedMesh.name != layerMeshNames[counter] || mc.sharedMesh.triangles.Count() != layerMeshPaintedTriangles[counter].Count()) {
					Mesh mesh = GetRawMesh(counter);
					mesh.Optimize();
					mc.sharedMesh = mesh;
					mc.convex = true;
				}
				counter++;
			}
		}

		internal Mesh GetRawMesh(int index) {
			var mesh = new Mesh();
			mesh.name = layerMeshNames[index];
			mesh.vertices = layerMeshVertices[index];
			mesh.triangles = layerMeshPaintedTriangles[index];
			return mesh;
		}

#if UNITY_EDITOR
		public Color[] layersColors;
		internal void UpdateTriangles(IEnumerable<ColliderLayer> layers) {
			layerMeshTriangles = layers.Select(l=>l?.triangles?.Values.ToArray()).ToArrayElemets();
			//layerMeshVertices = triangles.Select(l => (ArrayElement<Vector3>)(l?.SelectMany(t => t.Vertices())?.Distinct())?.ToArray()).ToArray();
			layerMeshVertices = layerMeshTriangles.Select(l=>l?.SelectMany(t=>t.Vertices()?.Distinct())).ToArrayElemets();
			layerMeshPaintedTriangles = new ArrayElement<int>[layerMeshVertices.Length];
			layerMeshNames = layers.Select(l=>l.name).ToArray();
			layersColors = layers.Select(l => l.color).ToArray();
			for (int i = 0; i < layerMeshPaintedTriangles.Length; i++) {
				var vertices = layerMeshVertices[i];

				layerMeshPaintedTriangles[i] = layerMeshTriangles[i]?
					.SelectMany(t => t.Vertices()
						.Select(v=> ArrayUtility.IndexOf(vertices,v)
					)
				).ToArray();

			}
			RegenerateColliders();
		}
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

		private zCollider obj;

		void OnEnable() {
			obj = serializedObject.targetObject as zCollider;
		}
	}
#endif

}
