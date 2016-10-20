#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class EditorInspectorUtils {
    /// <summary>
    /// Creates a new inspector window instance and locks it to inspect the specified target
    /// </summary>
    public static void InspectTarget(Object target) {
      // Get a reference to the `InspectorWindow` type object
      var inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
      // Create an InspectorWindow instance
      var inspectorInstance = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
      // We display it - currently, it will inspect whatever gameObject is currently selected
      // So we need to find a way to let it inspect/aim at our target GO that we passed
      // For that we do a simple trick:
      // 1- Cache the current selected gameObject
      // 2- Set the current selection to our target GO (so now all inspectors are targeting it)
      // 3- Lock our created inspector to that target
      // 4- Fallback to our previous selection
      inspectorInstance.Show();
      // Cache previous selected gameObject
      var prevSelection = Selection.activeGameObject;
      // Set the selection to GO we want to inspect
      Selection.activeObject = target;
      // Get a ref to the "locked" property, which will lock the state of the inspector to the current inspected target
      var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
      // Invoke `isLocked` setter method passing 'true' to lock the inspector
      isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
      // Finally revert back to the previous selection so that other inspectors continue to inspect whatever they were inspecting...
      Selection.activeGameObject = prevSelection;
    }

    [MenuItem("Assets/Inspect This &i", false, 20)]
    public static void InspectThis() {
      var o = Selection.activeObject;
      if (o) InspectTarget(o);
    }

    [MenuItem("Tools/Shortcuts/Break Prefab Instance &b")]
    public static void BreakPrefabInstance() {
      EditorApplication.ExecuteMenuItem("GameObject/Break Prefab Instance");
    }
  }
}
#endif