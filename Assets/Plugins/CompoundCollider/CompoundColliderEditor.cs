using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace ZorgsCompoundColliders {
	public class CompoundColliderEditor : OdinEditorWindow {
		public static CompoundColliderEditor Instance { get; private set; }

		public CompoundColliderSettingsScript currentObject;

		[OnCollectionChanged("StackChanged")]
		public List<ColliderSettings> settingsStack = new List<ColliderSettings>();

		[MenuItem("Window/zColliderEditor")]
		private static void OpenEditorWindow() {
			GetWindow<CompoundColliderEditor>().Show();
		}
		new private void OnEnable() {
			Instance = this;
			base.OnEnable();
			OnSelectionChange();
		}

		private void OnDisable() {
			Instance = null;
		}

		private void OnSelectionChange() {
			currentObject?.StopPainting();
			currentObject = Selection.activeGameObject?.GetComponent<CompoundColliderSettingsScript>();
			if (currentObject) currentObject.Editor = this;
			settingsStack = currentObject?.stack.ToList();
		}

		private void StackChanged() {
			if (settingsStack.Count < currentObject.stack.Count) {
				currentObject.RemoveFromStack(currentObject.stack.Where(s => !settingsStack.Any(ss => ss == s)));
			}
			currentObject.stack = settingsStack;
		}

		[Button]
		public void AddCollider() {
			currentObject = Selection.activeGameObject.GetComponent<CompoundColliderSettingsScript>();
			if (currentObject == null) {
				if (PrefabUtility.IsPartOfPrefabInstance(Selection.activeGameObject)) {
					PrefabUtility.RecordPrefabInstancePropertyModifications(Selection.activeObject);
					currentObject = Selection.activeGameObject.AddComponent<CompoundColliderSettingsScript>();
				} else if (!PrefabUtility.IsPartOfAnyPrefab(Selection.activeGameObject))
					currentObject = Selection.activeGameObject.AddComponent<CompoundColliderSettingsScript>();
				else
					return;
				currentObject.TryRestore();
				settingsStack = currentObject.stack.ToList();

				if (settingsStack.Count == 0)
					settingsStack.Add(currentObject.NewCollider($"New Layer {settingsStack.Count + 1}"));
			} else
				settingsStack.Add(currentObject.NewCollider($"New Layer {settingsStack.Count + 1}"));
		}

		[Button]
		public void GenerateColliders() {
			currentObject.GenerateColliders();
		}

		[Button]
		public void Clean() {
			currentObject.Clean();
			settingsStack = new List<ColliderSettings>();
			currentObject = null;
		}
	}
}