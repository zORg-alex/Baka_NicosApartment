using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace ZorgsCompoundColliders {
	/// <summary>
	/// Draws UI for painting (layers and stuff) (x)
	/// creates ZColliderPainterSCript object for painting, push input to it
	/// </summary>
	public class zColliderPainterEditorWindow : EditorWindow {

		static public Texture2D iconTexture;
		static public Texture2D negIconTexture;
		static public Texture2D paintbrushtexture;
		static bool guiInitialized => iconTexture != null && negIconTexture != null && paintbrushtexture != null;
		static void GUIInit() {
			iconTexture = Resources.Load<Texture2D>("eye");
			negIconTexture = Resources.Load<Texture2D>("eye_closed");
			paintbrushtexture = Resources.Load<Texture2D>("paintbrush");
		}

		public static zColliderEditor Instance { get; private set; }
		public static bool SubtractiveMode { get; internal set; }

		public static zCollider currentColliderScript;
		public static zColliderPainterScript currentPainter;

		[OnCollectionChanged("StackChanged")]
		public List<ColliderLayer> colliderLayers = new List<ColliderLayer>();

		public static bool IsPainting;
		public static bool ToolEnabled;

		[MenuItem("Tools/zColliderPainter")]
		public static void Enable() {
			if (!guiInitialized) GUIInit();
			ToolEnabled = !ToolEnabled;
			if (ToolEnabled) {
				SceneView.duringSceneGui += OnScene;
				Debug.Log("Scene GUI : Enabled");
			} else {
				SceneView.duringSceneGui -= OnScene;
				Debug.Log("Scene GUI : Disabled");
			}
		}

		private static void OnScene(SceneView sceneView) {
			Handles.BeginGUI();
			{
				GUILayout.BeginArea(new Rect(20, 20, 100, 200));
				{
					var rect = EditorGUILayout.BeginVertical();
					GUI.Box(rect, GUIContent.none);
					//Imitate toggle button
					GUI.color = IsPainting? Color.red: Color.white;
					//draw paint button, selectable layers, apply button, cleanup?
					if (GUILayout.Button(paintbrushtexture)) {
						Paint();
					}
					GUI.color = Color.white;

					if (currentPainter) {
						var painterSO = new SerializedObject(currentPainter);
						EditorGUILayout.PropertyField(painterSO.FindProperty("layers"));
					}

					EditorGUILayout.EndVertical();
				}
				GUILayout.EndArea();
			}
			Handles.EndGUI();
		}

		/// <summary>
		/// Starts painting on current zCollider/layer
		/// or ends if it's already on
		/// </summary>
		internal static void Paint() {
			var selectedZCollider = Selection.activeGameObject?.GetComponent<zCollider>();
			if (selectedZCollider != currentColliderScript)
				OnSelectionChange(selectedZCollider); //This shouldn't get hit, TODO remove
			IsPainting = !IsPainting;
			if (IsPainting) {
				AddPainter();
				AddColliderScript();

			} else {
				//Ended painting
			}
		}

		/// <summary>
		/// React on selection change
		/// </summary>
		private void OnSelectionChange() {
			if (Selection.activeGameObject == null) {
				currentColliderScript = null;
				//remove current painter?
				if (IsPainting) Paint();
			} else {
				if (IsPainting) Paint();
				currentColliderScript = Selection.activeGameObject.GetComponent<zCollider>();
				currentPainter?.SelectionChanged(currentColliderScript);
			}
		}

		//TODO remove
		private static void OnSelectionChange(zCollider selectedCollider) {
			//This shouldn't fire
			if (IsPainting)
				AddPainter();
			currentColliderScript = selectedCollider;
			currentPainter?.SelectionChanged(selectedCollider);
		}

		public static void AddColliderScript() {
			currentColliderScript = Selection.activeGameObject.GetComponent<zCollider>();
			if (currentColliderScript == null) {
				//TODO Optimize these ifs
				if (PrefabUtility.IsPartOfPrefabInstance(Selection.activeGameObject)) {
					PrefabUtility.RecordPrefabInstancePropertyModifications(Selection.activeObject);
					currentColliderScript = Selection.activeGameObject.AddComponent<zCollider>();
				} else if (!PrefabUtility.IsPartOfAnyPrefab(Selection.activeGameObject))
					currentColliderScript = Selection.activeGameObject.AddComponent<zCollider>();
				else
					return;
			}
		}

		internal static void PaintTriangle(Collider collider, int triangleIndex, bool substractive = false) {
			currentPainter.PaintTriangle(collider, triangleIndex, substractive);
		}

		public static void AddPainter() {
			if (currentPainter == null) {
				currentPainter = FindObjectOfType<zColliderPainterScript>();
			}
			if (currentPainter == null)
				currentPainter = new GameObject("zColliderPainter", typeof(zColliderPainterScript)).GetComponent<zColliderPainterScript>();
		}
	}
}