using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ZorgsCompoundColliders {
	/// <summary>
	/// Draws UI for painting (layers and stuff) (x)
	/// creates ZColliderPainterSCript object for painting, push input to it
	/// </summary>
	public class zColliderPainterEditorWindow : EditorWindow {

		static public Texture2D paintbrushtexture;
		static bool guiInitialized => paintbrushtexture != null;
		static void GUIInit() {
			paintbrushtexture = Resources.Load<Texture2D>("paintbrush");
		}

		public static zColliderEditor Instance { get; private set; }
		public static bool SubtractiveMode { get; internal set; }

		public static zCollider currentColliderScript;
		public static zColliderPainterScript currentPainter;

		[OnCollectionChanged("StackChanged")]
		public List<ColliderLayer> colliderLayers = new List<ColliderLayer>();

		public static ColliderLayer currentLayer { get => currentPainter.paintedColliderLayer; set => currentPainter.paintedColliderLayer = value; }
		//TODO remove if unused
		public static int currentLayerIndex => currentPainter.layers.IndexOf(currentLayer);

		public static bool IsPainting { get => currentPainter?.isPainting ?? false; set { currentPainter.isPainting = value; } }
		public static bool ToolEnabled;
		public static ReorderableList Layerlist { get; private set; }
		
		[MenuItem("Tools/zColliderPainter")]
		public static void ToggleTool() {
			if (!guiInitialized) GUIInit();
			ToolEnabled = !ToolEnabled;
			currentColliderScript = Selection.activeGameObject?.GetComponent<zCollider>();
			AddPainter();
			if (ToolEnabled) {
				SceneView.duringSceneGui += OnScene;
				Selection.selectionChanged += OnSelectionChange;
			} else {
				SceneView.duringSceneGui -= OnScene;
				Selection.selectionChanged -= OnSelectionChange;
				StopPaint();
				Utils.Destroy(currentPainter.gameObject);
			}
		}

		private static void InitUI() {
			Layerlist = new ReorderableList(currentPainter.layers, typeof(ColliderLayer), false, false, true, true);

			Layerlist.onAddCallback += l => {
				if (!currentPainter)
					AddPainter();
				if (!currentColliderScript)
					AddColliderScript();
				currentPainter.AddColliderLayer("Layer " + (currentPainter.layers.Count + 1));
			};
			Layerlist.onRemoveCallback += l => currentPainter.RemoveColliderLayer(currentPainter.layers[l.index]);

			Layerlist.drawElementCallback += (rect, ind, act, foc) => {
				if (IsPainting)
					ColliderLayerDrawer.DefaultOnGUI(currentPainter.layers[ind], new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4));
				else
					EditorGUI.TextField(new Rect(rect.x, rect.y + 2, rect.width, rect.height - 4), ((ColliderLayer)Layerlist.list[ind]).name);
			};
			Layerlist.onSelectCallback += l => {
				currentLayer = currentPainter.layers[l.index];
			};
			Layerlist.elementHeight = 22;
		}

		private static void OnScene(SceneView sceneView) {
			Handles.BeginGUI();
			{
				GUILayout.BeginArea(new Rect(20, 20, 270, Screen.height));
				{
					var boxRect = EditorGUILayout.BeginVertical();
					EditorGUILayout.BeginHorizontal();
					//GUI.Box(boxRect, GUIContent.none);
					//Imitate toggle button
					GUI.color = IsPainting ? Color.red : Color.white;
					//draw paint button, selectable layers, apply button, cleanup?
					if (GUILayout.Button(paintbrushtexture, GUILayout.Width(100))) {
						PaintToggle();
					}
					GUI.color = Color.white;

					GUILayout.FlexibleSpace();
					if (GUILayout.Button("X", GUILayout.Width(22)))
						ToggleTool();

					EditorGUILayout.EndHorizontal();

					if (currentPainter != null) try {
						Layerlist?.DoLayoutList();
					} catch (Exception e) { }

					EditorGUILayout.EndVertical();
				}
				GUILayout.EndArea();
			}
			Handles.EndGUI();

			if (currentPainter && currentColliderScript) {

				Event current = Event.current;
				int controlID = GUIUtility.GetControlID(currentPainter.pathEditorHash, FocusType.Passive);

				// If we are in edit mode and the user clicks (right click, middle click or alt+left click)
				if (Application.isEditor && zColliderPainterEditorWindow.IsPainting) {

					if (current.type == EventType.Layout)
						//Magic thing
						HandleUtility.AddDefaultControl(controlID);
					else if ((current.type == EventType.MouseDrag || current.type == EventType.MouseDown) && current.button == 0) {
						Ray worldRay = Camera.current.ScreenPointToRay(new Vector3(current.mousePosition.x, Screen.height - current.mousePosition.y - 36, 0));
						zColliderPainterEditorWindow.PaintWithRay(worldRay);
						current.Use();//This will prevent selection change while painting
					} else if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape) {
						zColliderPainterEditorWindow.StopPaint();
						current.Use();
					} else if (current.type == EventType.KeyDown && current.keyCode == KeyCode.LeftControl)
						zColliderPainterEditorWindow.SubtractiveMode = true;
					else if (current.type == EventType.KeyUp && current.keyCode == KeyCode.LeftControl)
						zColliderPainterEditorWindow.SubtractiveMode = false;

				}
			}
			currentPainter.IsNull()?.SetDrawGizmos(IsPainting);
		}

		private int pathEditorHash;

		void OnEnable() {
			pathEditorHash = this.GetHashCode();
		}

		/// <summary>
		/// Starts painting on current zCollider/layer
		/// or ends if it's already on
		/// </summary>
		internal static void PaintToggle() => Paint(!IsPainting);
		internal static void Paint(bool isPainting = true){
			var selectedZCollider = Selection.activeGameObject?.GetComponent<zCollider>();
			if (!Selection.activeGameObject) { StopPaint(); return; }
			if (selectedZCollider != currentColliderScript) {
				currentColliderScript = selectedZCollider;
			}
			IsPainting = isPainting;
			if (IsPainting) {
				AddPainter();
				AddColliderScript();
				if (currentLayer == null) {
					currentPainter.AddColliderLayer("Layer " + (currentPainter.layers.Count + 1));
					InitUI();
				}
			} else {
				StopPaint();
			}
		}

		internal static void StopPaint() {
			IsPainting = false;

			//Painting stopped, apply?
			currentPainter.IsNull()?.ApplyColliderChanges();
		}

		/// <summary>
		/// Used in Layer UI. Painter should be already present, so as zCollider
		/// </summary>
		/// <param name="layer"></param>
		internal static void Paint(ColliderLayer layer) {
			IsPainting = true;
			currentPainter.IsNull()?.ApplyColliderChanges();
			currentLayer = layer;
			Layerlist.index = Layerlist.list.IndexOf(layer);
		}

		/// <summary>
		/// React on selection change
		/// </summary>
		private static void OnSelectionChange() {
			StopPaint();
			currentPainter.IsNull()?.Clean();
			if (Selection.activeGameObject) {
				currentColliderScript = Selection.activeGameObject.GetComponent<zCollider>();
				AddPainter();
			} else {
				//currentColliderScript = null;
				//currentPainter.IsNull()?.SelectionChangedTo(currentColliderScript);
			}
			InitUI();
		}

		public static void AddColliderScript() {
			if (Selection.activeGameObject == null) return;
			currentColliderScript = Selection.activeGameObject?.GetComponent<zCollider>();
			if (currentColliderScript == null) {
				//TODO Optimize these ifs
				if (PrefabUtility.IsPartOfPrefabInstance(Selection.activeGameObject)) {
					PrefabUtility.RecordPrefabInstancePropertyModifications(Selection.activeObject);
					currentColliderScript = Selection.activeGameObject.AddComponent<zCollider>();
				} else if (!PrefabUtility.IsPartOfAnyPrefab(Selection.activeGameObject))
					currentColliderScript = Selection.activeGameObject.AddComponent<zCollider>();
				else
					return;
				//Restore layers if added new zCollider
			}
			currentPainter.SelectionChangedTo(currentColliderScript);
		}

		internal static void PaintWithRay(Ray ray) {
			currentPainter.PaintWithRay(ray, currentLayer, SubtractiveMode);
		}

		public static void AddPainter() {
			if (currentPainter == null) {
				currentPainter = FindObjectOfType<zColliderPainterScript>();
			}
			if (currentPainter == null)
				currentPainter = new GameObject("zColliderPainter", typeof(zColliderPainterScript)).GetComponent<zColliderPainterScript>();

			currentPainter.SelectionChangedTo(currentColliderScript);

			if (!currentPainter.layers.Contains(currentLayer) || currentLayer == null)
				currentLayer = currentPainter.layers.FirstOrDefault();
			//Should reinit when list changes
			InitUI();
			if (currentLayer == null)
				Layerlist.index = currentPainter.layers.IndexOf(currentLayer);
		}
	}
}