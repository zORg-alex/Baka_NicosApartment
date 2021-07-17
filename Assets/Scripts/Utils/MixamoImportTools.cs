using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class MixamoImportTools : OdinEditorWindow {
	[MenuItem("Window/Mixamo Import Tools")]
	private static void OpenEditorWindow() {
		GetWindow<MixamoImportTools>().Show();
	}
	[Button("Fix naming in Animations")]
	void FixNamingInAnimations() {
		foreach (GameObject o in Selection.objects) {
			ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(o));
			var clips = modelImporter.clipAnimations;
			foreach (var c in clips) {
				if (c.name == "mixamo.com") c.name = o.name;
			}
			modelImporter.clipAnimations = clips;
		}
	}
}
