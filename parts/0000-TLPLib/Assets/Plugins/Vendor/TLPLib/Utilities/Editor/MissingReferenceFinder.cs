using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissingReferencesFinder : MonoBehaviour {
  [MenuItem("Tools/Show Missing Object References in scene", false, 50)]
  public static void findMissingReferencesInCurrentScene() {
    var objects = getSceneObjects();
    findMissingReferences(SceneManager.GetActiveScene().name, objects);
    Debug.Log("findMissingReferencesInCurrentScene finished");
  }

  [MenuItem("Tools/Show Missing Object References in all scenes", false, 51)]
  public static void missingReferencesInAllScenes() {
    var scenes = AssetDatabase.GetAllAssetPaths().Where(p => p.EndsWith(".unity")).ToArray();
    for (var i = 0; i < scenes.Length; i++) {
      var scene = scenes[i];
      if (EditorUtility.DisplayCancelableProgressBar("missingReferencesInAllScenes", scene, (float) i++ / scenes.Length)) {
        break;
      }
      EditorSceneManager.OpenScene(scene);
      findMissingReferences(scene, getSceneObjects(), false);
    }
    EditorUtility.ClearProgressBar();
    Debug.Log("missingReferencesInAllScenes finished");
  }

  [MenuItem("Tools/Show Missing Object References in assets", false, 52)]
  public static void missingReferencesInAssets() {
    EditorUtility.DisplayProgressBar("missingReferencesInAssets", "missingReferencesInAssets", 0);

    var allAssets = AssetDatabase.GetAllAssetPaths();
    var objs = allAssets.Select(a => AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null).ToArray();

    findMissingReferences("Project", objs);
    Debug.Log("missingReferencesInAssets finished");
  }

  static void findMissingReferences(string context, GameObject[] objects, bool useProgress = true) {
    var scanned = 0;
    var missingComponents = 0;
    var missingRefs = 0;
    var nullRefs = 0;
    foreach (var go in objects) {
      if (useProgress) {
        EditorUtility.DisplayProgressBar("findMissingReferences", "findMissingReferences", (float) scanned++ / objects.Length);
      }
      var components = go.GetComponentsInChildren<Component>();

      foreach (var c in components) {
        if (!c) {
          missingComponents++;
          Debug.LogError("Missing Component in GO or children: " + fullPath(go), go);
          continue;
        }

        var so = new SerializedObject(c);
        var sp = so.GetIterator();

        while (sp.NextVisible(true)) {
          if (sp.propertyType == SerializedPropertyType.ObjectReference) {
            if (sp.objectReferenceValue == null
                && sp.objectReferenceInstanceIDValue != 0) {
              missingRefs++;
              showError(ERR, context, c.gameObject, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
            }
          }
        }

        var notNullFields = c.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(fi => fi.GetCustomAttributes(typeof(NotNullAttribute), false).Length != 0);

        foreach (var field in notNullFields) {
          if ((field.GetValue(c) as Object) == null) {
            nullRefs++;
            showError(ERR2, context, c.gameObject, c.GetType().Name, field.Name);
          }
        }
      }
    }
    if (useProgress) {
      EditorUtility.ClearProgressBar();
    }
    if (missingComponents + missingRefs + nullRefs > 0) {
      Debug.LogError($"[{context}] Missing components: {missingComponents} Missing Refs: {missingRefs} Null Refs: {nullRefs}");
    }
  }

  static GameObject[] getSceneObjects() {
    return SceneManager.GetActiveScene().GetRootGameObjects()
        .Where(go => go.hideFlags == HideFlags.None).ToArray();
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