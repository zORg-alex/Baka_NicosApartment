using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	[CustomPropertyDrawer(typeof(ColliderSettings))]
	public class ColliderSettingsDrawer : PropertyDrawer {

		public Texture2D iconTexture;
		public Texture2D negIconTexture;
		public Texture2D paintbrushtexture;
		void Init() {
			iconTexture = Resources.Load<Texture2D>("eye");
			negIconTexture = Resources.Load<Texture2D>("eye_closed");
			paintbrushtexture = Resources.Load<Texture2D>("paintbrush");
			initialized = true;
		}

		bool initialized;
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return -2f; // this seems to match closest to non-property drawer version
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (!initialized) Init();
			var obj = property.GetValue<ColliderSettings>();
			GUILayout.Space(-position.height);
			GUILayout.BeginHorizontal();

			var oldName = obj.name;
			var oldShowGiz = obj.ShowGizmo;
			obj.name = GUILayout.TextField(obj.name);

			if (obj.ShowGizmo)
				obj.ShowGizmo = !GUILayout.Button(new GUIContent(iconTexture), GUILayout.Width(28), GUILayout.Height(24));
			else
				obj.ShowGizmo = GUILayout.Button(new GUIContent(negIconTexture), GUILayout.Width(28), GUILayout.Height(24));

			if (oldName != obj.name || oldShowGiz != obj.ShowGizmo)
				EditorUtility.SetDirty(property.serializedObject.targetObject);

			if (GUILayout.Button(new GUIContent(paintbrushtexture), GUILayout.Width(50), GUILayout.Height(24)))
				obj.Paint();
			GUILayout.EndHorizontal();
		}
	}
}
