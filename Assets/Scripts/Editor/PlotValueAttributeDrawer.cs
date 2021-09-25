using System;
using UnityEditor;
using UnityEngine;
using Utility;

[CustomPropertyDrawer(typeof(PlotValueAttribute))]
public class PlotValueDrawer : PropertyDrawer {
	public Texture2D texture;
	public PlotValueAttribute plotAttribute;

	bool initialized;
	float[,] prevValues;
	int lastStep;
	int currentStep;
	float minValue = -0.00001f;
	float maxValue = 0.00001f;
	private float range { get { return maxValue - minValue; } }
	Color32 gridColor = new Color(.5f, .5f, .5f, .5f);
	Color32 background = new Color(0, 0, 0, 0);
	Color32 XColor = new Color(1, 0, 0, 1);
	Color32 YColor = new Color(0, 1, 0, 1);
	Color32 ZColor = new Color(0, 0, 1, 1);
	private Color32[] backgroundColorArray;

	public Action<int,int> PlotValues { get; private set; }

	public void Init(SerializedProperty property) {
		plotAttribute = (PlotValueAttribute)attribute;
		if (plotAttribute == null) return;
		texture = new Texture2D(plotAttribute.Size.x, plotAttribute.Size.y);
		backgroundColorArray = texture.GetPixels32();
		int firstRow = texture.width;
		int lastRowBeginning = (texture.height - 1) * texture.width;
		for (int i = 0; i < backgroundColorArray.Length; i++) {
			if (i < firstRow || i > lastRowBeginning)
				backgroundColorArray[i] = gridColor;
			else
				backgroundColorArray[i] = background;
		}

		PlotValues = (x1, x2) => { };
		if (property.type == "Float") {
			prevValues = new float[plotAttribute.Size.x, 1];
			PlotValues = (x1,x2) => {
				DrawLine(x1, prevValues[x1, 0], x2, prevValues[x2, 0], Color.white);
			};
		} else if (property.type == "Vector2") {
			prevValues = new float[plotAttribute.Size.x, 2];
			PlotValues = (x1, x2) => {
				DrawLine(x1, prevValues[x1, 0], x2, prevValues[x2, 0], XColor);
				DrawLine(x1, prevValues[x1, 1], x2, prevValues[x2, 1], YColor);
			};
		} else if (property.type == "Vector3") {
			prevValues = new float[plotAttribute.Size.x, 3];
			PlotValues = (x1, x2) => {
				DrawLine(x1, prevValues[x1, 0], x2, prevValues[x2, 0], XColor);
				DrawLine(x1, prevValues[x1, 1], x2, prevValues[x2, 1], YColor);
				DrawLine(x1, prevValues[x1, 2], x2, prevValues[x2, 2], ZColor);
			};
		}

		initialized = true;
	}

	private void DrawLine(int x1, float v1, int x2, float v2, Color color) {
		if (x2 - x1 == 1)
			texture.DrawLine(
				new Vector2(x1, (v1 - minValue) / range * (texture.height - 1)),
				new Vector2(x2, (v2 - minValue) / range * (texture.height - 1)), color);
	}
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		if (!initialized) Init(property);
		return plotAttribute.Size.y;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		if (!initialized) Init(property);
		GUI.Label(new Rect(position.x,position.y, position.width - texture.width - 5, 22), label);
		EditorGUI.PropertyField(new Rect(position.x, position.y + 25, position.width - texture.width - 5, 22), property, GUIContent.none);

		var plotRect = new Rect(position.x + position.width - texture.width, position.y, texture.width, position.height);

		if (Event.current.type == EventType.Repaint) {
			SaveValue(property);
			lastStep = currentStep++;
			if (currentStep >= plotAttribute.Size.x) currentStep = 0;
			DrawTexture();
			EditorGUI.DrawPreviewTexture(plotRect, texture);
		}

		GUI.Label(new Rect(position.x + position.width - texture.width + 3, position.y, texture.width -6, 18), maxValue.ToString());
		GUI.Label(new Rect(position.x + position.width - texture.width + 3, position.y + position.height - 18, texture.width - 6, 18), minValue.ToString());
	}

	private void DrawTexture() {
		texture.SetPixels32(backgroundColorArray);
		texture.DrawLine(new Vector2(0, -minValue / range * texture.height), new Vector2(texture.width - 1, -minValue / range * texture.height), gridColor);
		for (int i = 0; i < prevValues.GetLength(0); i++) {
			PlotValues((i + lastStep) % prevValues.GetLength(0), (i + currentStep) % prevValues.GetLength(0));
		}

  		texture.Apply();
	}

	private void SaveValue(SerializedProperty property) {
		if (property.type == "Float") {
			prevValues[currentStep, 0] = property.floatValue;
			minValue = Mathf.Min(minValue, property.floatValue);
			maxValue = Mathf.Max(maxValue, property.floatValue);
		} else if (property.type == "Vector2") {
			prevValues[currentStep, 0] = property.vector2Value.x;
			prevValues[currentStep, 1] = property.vector2Value.y;
			minValue = Mathf.Min(Mathf.Min(minValue, property.vector2Value.x), property.vector2Value.y);
			maxValue = Mathf.Max(Mathf.Max(maxValue, property.vector2Value.x), property.vector2Value.y);
		} else if (property.type == "Vector3") {
			prevValues[currentStep, 0] = property.vector3Value.x;
			prevValues[currentStep, 1] = property.vector3Value.y;
			prevValues[currentStep, 2] = property.vector3Value.z;
			minValue = Mathf.Min(Mathf.Min(Mathf.Min(minValue, property.vector3Value.x), property.vector3Value.y), property.vector3Value.z);
			maxValue = Mathf.Max(Mathf.Max(Mathf.Max(maxValue, property.vector3Value.x), property.vector3Value.y), property.vector3Value.z);
		}
	}
}