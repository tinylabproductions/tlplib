#if UNITY_EDITOR
using System.Reflection;
using com.tinylabproductions.TLPLib.reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Editor.Utils {
  public static class EditorInspectorUtils {
    /// <summary>
    /// Creates a new inspector window instance and locks it to inspect the specified target
    /// </summary>
    [PublicAPI]
    public static void inspectTarget(Object target) {
      // Get a reference to the `InspectorWindow` type object
      var inspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
      // Create an InspectorWindow instance
      var inspectorInstance = ScriptableObject.CreateInstance(inspectorType) as EditorWindow;
      Debug.Assert(inspectorInstance != null, nameof(inspectorInstance) + " != null");
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
      // Get a ref to the public "locked" property, which will lock the state of the inspector to the current inspected target
      var isLocked = inspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
      // Invoke `isLocked` setter method passing 'true' to lock the inspector
      Debug.Assert(isLocked != null, nameof(isLocked) + " != null");
      isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
      // Finally revert back to the previous selection so that other inspectors continue to inspect whatever they were inspecting...
      Selection.activeGameObject = prevSelection;
    }

    [MenuItem("Assets/TLP/Inspect This &i", isValidateFunction: false, priority: 20)]
    public static void inspectThis() {
      var o = Selection.activeObject;
      if (o) inspectTarget(o);
    }
    
    /// <summary>
    /// Creates a new project window instance and locks it to inspect the specified target
    /// </summary>
    [PublicAPI]
    public static void projectWindowForTarget(Object target) {
      // Get a reference to the `InspectorWindow` type object
      var windowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
      var windowInstance = ScriptableObject.CreateInstance(windowType) as EditorWindow;
      Debug.Assert(windowInstance != null, nameof(windowInstance) + " != null");
      windowInstance.Show();
      PrivateMethod.obtain(windowType, "Init")(windowInstance);

      var getInstanceIDFromGUID = PrivateMethod.obtainStaticFn<string, int>(
        typeof(AssetDatabase), "GetInstanceIDFromGUID"
      );
      
      var setFolderSelection = PrivateMethod.obtain<int[], bool>(windowType, "SetFolderSelection");
      setFolderSelection(
        windowInstance, 
        new[] {getInstanceIDFromGUID(
          AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject))
        )},
        true
      );
      var isLockedFieldAccessor = PrivateField.accessor<bool>(windowType, "m_IsLocked"); 
      var isLockedField = isLockedFieldAccessor(windowInstance);
      isLockedField.value = true;
    }

    [MenuItem("Assets/TLP/Project Tab for This &p", isValidateFunction: false, priority: 20)]
    public static void projectTabForThis() {
      var o = Selection.activeObject;
      if (o) projectWindowForTarget(o);
    }
    
    [MenuItem("Assets/TLP/Toggle Active State &e", priority = 20)]
    public static void toggleActiveState() {
      var objects = Selection.gameObjects;
      Undo.RegisterCompleteObjectUndo(objects, "Toggle Active State");
      foreach (var go in objects) go.SetActive(!go.activeSelf);
    }

    [MenuItem("TLP/Tools/Shortcuts/Break Prefab Instance &b")]
    public static void breakPrefabInstance() {
      EditorApplication.ExecuteMenuItem("GameObject/Break Prefab Instance");
    }
  }
}
#endif