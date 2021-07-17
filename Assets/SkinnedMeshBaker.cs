using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class SkinnedMeshBaker : MonoBehaviour
{
	public Mesh bakeResult;
	[Button]
	public void Bake() {
		var pos = transform.position;
		var rot = transform.rotation;
		var par = transform.parent;
		transform.parent = null;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;

		var smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
		if (smrs.Length > 0) {
			var combMesh = new CombineInstance[smrs.Length];
			var i = 0;
			foreach (var smr in smrs) {
				var mesh = new Mesh();
				smr.BakeMesh(mesh);
				//var vv = mesh.vertices;
				//for (int j = 0; j < vv.Length; j++) {
				//	vv[j] = transform.TransformPoint(vv[j]);
				//}
				//mesh.vertices = vv;
				combMesh[i].mesh = mesh;
				combMesh[i].transform = smr.transform.localToWorldMatrix;
				i++;
			}
			bakeResult = new Mesh() { name = this.name + "_baked" };
			bakeResult.CombineMeshes(combMesh);

			AssetDatabase.CreateAsset(bakeResult, "Assets/"+this.name + "_baked.asset");
		}

		transform.parent = par;
		transform.position = pos;
		transform.rotation = rot;
	}
}
