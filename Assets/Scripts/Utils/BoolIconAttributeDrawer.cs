using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BoolIconAttribute))]
public class BoolIconAttributeDrawer : PropertyDrawer {
	BoolIconAttribute BoolIcon => (BoolIconAttribute)attribute;

	private const string TextureNotFoundMessage = "Icon texture wasn't found.";
	public Texture2D iconTexture;
	public Texture2D negIconTexture;

	public bool Initialized { get; private set; }
	void Init() {
		if (iconTexture == null) {
			iconTexture = Resources.Load<Texture2D>(BoolIcon.icon);
			if (iconTexture == null)
				Debug.LogError(TextureNotFoundMessage);
		}
		if (negIconTexture == null) {
			negIconTexture = Resources.Load<Texture2D>(BoolIcon.negativeIcon);
			if (negIconTexture == null)
				Debug.LogError(TextureNotFoundMessage);
		}
		Initialized = true;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!Initialized) Init();

		if (property.propertyType != SerializedPropertyType.Boolean) {
			base.OnGUI(position, property, label);
			Debug.LogError("BoolIcon should be used on bool properties");
		} else if (property.boolValue)
			property.boolValue = !GUI.Button(position, new GUIContent(iconTexture));
		else
			property.boolValue = GUI.Button(position, new GUIContent(negIconTexture));
	}
}