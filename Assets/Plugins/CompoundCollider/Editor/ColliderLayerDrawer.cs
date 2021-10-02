using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	[CustomPropertyDrawer(typeof(ColliderLayer))]
	public class ColliderLayerDrawer : PropertyDrawer {

		static public Texture2D iconTexture;
		static public Texture2D negIconTexture;
		static public Texture2D paintbrushtexture;
		static bool initialized => iconTexture != null && negIconTexture != null && paintbrushtexture != null;
		void Init() {
			iconTexture = Resources.Load<Texture2D>("eye");
			negIconTexture = Resources.Load<Texture2D>("eye_closed");
			paintbrushtexture = Resources.Load<Texture2D>("paintbrush");
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 0;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (!initialized) Init();
			var obj = property.GetValue<ColliderLayer>();

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
			obj.color = EditorGUILayout.ColorField(obj.color);

			GUILayout.EndHorizontal();
		}
	}
}
