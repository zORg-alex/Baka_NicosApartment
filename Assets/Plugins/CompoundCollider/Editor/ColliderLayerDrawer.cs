using System;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	[CustomPropertyDrawer(typeof(ColliderLayer))]
	public class ColliderLayerDrawer : PropertyDrawer {

		static public Texture2D eyeTexture;
		static public Texture2D clEyeTexture;
		static public Texture2D paintbrushtexture;
		static bool initialized => eyeTexture != null && clEyeTexture != null && paintbrushtexture != null;
		static void Init() {
			eyeTexture = Resources.Load<Texture2D>("eye");
			clEyeTexture = Resources.Load<Texture2D>("eye_closed");
			paintbrushtexture = Resources.Load<Texture2D>("paintbrush");
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return 24f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var obj = property.GetValue<ColliderLayer>();
			var oldName = obj.name;
			var oldShowGiz = obj.ShowGizmo;
			DefaultOnGUI(obj, position);
			if (oldName != obj.name || oldShowGiz != obj.ShowGizmo)
				EditorUtility.SetDirty(property.serializedObject.targetObject);

		}

		public static void DefaultOnGUI(ColliderLayer obj, Rect position) {
			if (!initialized) Init();
			var rect = new Rect(position.x, position.y, 100, position.height);
			obj.name = GUI.TextField(rect, obj.name);

			rect.x += rect.width + 5;
			rect.width = 28;
			obj.ShowGizmo = !GUI.Button(rect, new GUIContent(obj.ShowGizmo ? eyeTexture : clEyeTexture));

			rect.x += rect.width + 5;
			rect.width = 50;
			if (GUI.Button(rect, new GUIContent(paintbrushtexture)))
				obj.Paint();

			rect.x += rect.width + 5;//WTF?
			rect.width = 50;
			obj.color = EditorGUI.ColorField(rect, GUIContent.none, obj.color);
		}
	}
}
