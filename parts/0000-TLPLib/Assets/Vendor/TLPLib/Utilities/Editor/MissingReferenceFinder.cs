using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class MissingReferencesFinder : MonoBehaviour {
  [MenuItem("Tools/Show Missing Object References in scene", false, 50)]
  public static void findMissingReferencesInCurrentScene() {
    var objects = getSceneObjects();
    findMissingReferences(EditorApplication.currentScene, objects);
  }

  [MenuItem("Tools/Show Missing Object References in all scenes", false, 51)]
  public static void missingSpritesInAllScenes() {
    foreach (var scene in EditorBuildSettings.scenes) {
      EditorApplication.OpenScene(scene.path);
      findMissingReferences(scene.path, Resources.FindObjectsOfTypeAll<GameObject>());
    }
  }

  [MenuItem("Tools/Show Missing Object References in assets", false, 52)]
  public static void missingSpritesInAssets() {
    var allAssets = AssetDatabase.GetAllAssetPaths();
    var objs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

    findMissingReferences("Project", objs);
  }

  static void findMissingReferences(string context, GameObject[] objects) {
    foreach (var go in objects) {
      var components = go.GetComponents<Component>();

      foreach (var c in components) {
        if (!c) {
          Debug.LogError("Missing Component in GO: " + fullPath(go), go);
          continue;
        }

        var so = new SerializedObject(c);
        var sp = so.GetIterator();

        while (sp.NextVisible(true)) {
          if (sp.propertyType == SerializedPropertyType.ObjectReference) {
            if (sp.objectReferenceValue == null
                && sp.objectReferenceInstanceIDValue != 0) {
              showError(ERR, context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
            }
          }
        }

        var notNullFields = c.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(fi => fi.GetCustomAttributes(typeof(NotNullAttribute), false).Length != 0);

        foreach (var field in notNullFields) {
          if ((field.GetValue(c) as Object) == null) {
            showError(ERR2, context, go, c.GetType().Name, field.Name);
          }
        }
      }
    }
  }

  static GameObject[] getSceneObjects() {
    return Resources.FindObjectsOfTypeAll<GameObject>()
        .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
               && go.hideFlags == HideFlags.None).ToArray();
  }

  const string ERR = "Missing Ref in: [{3}]{0}. Component: {1}, Property: {2}";
  const string ERR2 = "Null Ref in: [{3}]{0}. Component: {1}, Property: {2}";

  static void showError(string errorFormat, string context, GameObject go, string c, string property) {
    Debug.LogError(string.Format(errorFormat, fullPath(go), c, property, context), go);
  }

  static string fullPath(GameObject go) {
    return go.transform.parent == null
        ? go.name
            : fullPath(go.transform.parent.gameObject) + "/" + go.name;
  }
}