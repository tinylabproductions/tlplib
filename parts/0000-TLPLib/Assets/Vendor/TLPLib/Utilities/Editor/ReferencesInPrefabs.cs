using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine.Events;

namespace Assets.Vendor.TLPLib.Utilities.Editor {
  public class ReferencesInPrefabs : MonoBehaviour {
    const string NAMESPACE_NAME = "com.tinylabproductions";
    static bool anyErrors;

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
      if (anyErrors) {
        anyErrors = false;
        throw new Exception("Build failed");
      }
    }

    [PostProcessScene(0)]
    [MenuItem("Tools/Show Missing Object References in scene2", false, 55)]
    public static void findMissingReferencesInCurrentScene() {
      GameObject[] objects = EditorApplication.isPlayingOrWillChangePlaymode
        ? Resources.FindObjectsOfTypeAll<GameObject>().Where(o => !AssetDatabase.Contains(o)).ToArray()
        : getSceneObjects();
      var errors = findMissingReferences(SceneManager.GetActiveScene().name, objects);
      foreach (var error in errors) showError(error);
      anyErrors = errors.Any();
      Debug.Log("findMissingReferencesInCurrentScene finished");
    }

    //[MenuItem("Tools/Show Missing Object References in assets2", false, 57)]
    public static void missingReferencesInAssets() {
      var allAssets = AssetDatabase.GetAllAssetPaths();
      var objs =
        allAssets.Select(assetPath => AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject)
          .Where(a => a != null).ToArray();
      findMissingReferences("Project", objs);
      Debug.Log("missingReferencesInAssets finished");
    }

    static List<Tpl<string, GameObject>> findMissingReferences(string context, GameObject[] objects, bool useProgress = true) {
      var errors = new List<Tpl<string, GameObject>>();
      var scanned = 0;
      foreach (var go in objects) {
        if (useProgress) {
          EditorUtility.DisplayProgressBar("findMissingReferences", "findMissingReferences", (float)scanned++ / objects.Length);
        }
        var components = go.GetComponentsInChildren<Component>();

        foreach (var c in components) {
          if (!c) {
            errors.Add(createError(ERR3, c.gameObject));
            continue;
          }

          var so = new SerializedObject(c);
          var sp = so.GetIterator();

          while (sp.NextVisible(true)) {
            if (sp.propertyType == SerializedPropertyType.ObjectReference) {
              if (sp.objectReferenceValue == null
                  && sp.objectReferenceInstanceIDValue != 0) {
                errors.Add(createError(ERR, context, c.gameObject, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name)));
              }
            }
            if (sp.type == "UnityEvent") {
              var a = getUnityEvent(c, sp.propertyPath);
              var baseType = a.GetType().BaseType;
              var method = baseType.GetMethod("RebuildPersistentCallsIfNeeded", BindingFlags.Instance | BindingFlags.NonPublic);
              method.Invoke(a, new object[] { });

              var m_PersistentCalls = baseType.GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
              var m_PersistentCallsValue = m_PersistentCalls.GetValue(a);

              var m_Calls = m_PersistentCallsValue.GetType().GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
              var m_CallsValue = m_PersistentCalls.GetValue(a);

              //var methodInfo = baseType.GetMethod("FindMethod", BindingFlags.Instance | BindingFlags.NonPublic);
              var methods = baseType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
              var methodInfo = methods.First(mi => mi.Name.Equals("FindMethod") && mi.GetParameters().Any());

              IEnumerable listObject = (IEnumerable)m_Calls.GetValue(m_CallsValue);
              foreach (var persistentCall in listObject) {
                var isValid = (bool)persistentCall.GetType().GetMethod("IsValid").Invoke(persistentCall, new object[] { });
                if (isValid) {
                  //Debug.LogWarning(a);
                  //Debug.LogWarning(methodInfo);
                  //Debug.LogWarning(persistentCall);

                  var mi = methodInfo.Invoke(a, new[] { persistentCall });
                  Debug.LogWarning(mi);
                  if (mi == null) {
                    Debug.LogWarning(sp.propertyPath);
                  }
                }
              }
              var prop = m_PersistentCallsValue.GetType().GetProperty("Count");
              var count = (int)prop.GetValue(m_PersistentCallsValue, new object[] { });
              //Debug.LogWarning(count);
              //Debug.LogWarning(a.GetPersistentEventCount());
            }
          }

          var notNullFields = ReferencesInPrefabs.notNullFields(c.GetType());

          foreach (var field in notNullFields) {
            if ((field.GetValue(c) as UnityEngine.Object) == null) {
              errors.Add(createError(ERR2, context, c.gameObject, c.GetType().Name, field.Name));
            }
          }
        }
      }
      if (useProgress) {
        EditorUtility.ClearProgressBar();
      }
      return errors;
    }

    static UnityEvent getUnityEvent(object obj, string fieldName) {
      if (obj != null) {
        FieldInfo fi = obj.GetType().GetField(fieldName);
        if (fi != null) {
          return fi.GetValue(obj) as UnityEvent;
        }
      }
      return null;
    }

    static IEnumerable<FieldInfo> notNullFields(Type type) {
      var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic);
      if (type.Namespace?.StartsWith(NAMESPACE_NAME) ?? false) {
        return fields.Where(fi => fi.GetCustomAttributes(typeof(CanBeNullAttribute), false).Length == 0);
      }
      return fields.Where(fi => fi.GetCustomAttributes(typeof(NotNullAttribute), false).Length != 0);
    }

    static GameObject[] getSceneObjects() {
      return SceneManager.GetActiveScene().GetRootGameObjects()
        .Where(go => go.hideFlags == HideFlags.None).ToArray();
    }

    const string ERR = "Missing Ref in: [{3}]{0}. Component: {1}, Property: {2}";
    const string ERR2 = "Null Ref in: [{3}]{0}. Component: {1}, Property: {2}";
    const string ERR3 = "Missing Component in GO or children: {0}";

    static void showError(Tpl<string, GameObject> error) { Debug.LogError(error._1, error._2); }

    static Tpl<string, GameObject> createError(string errorFormat, string context, GameObject go, string c, string property) {
      return F.t(string.Format(errorFormat, fullPath(go), c, property, context), go);
    }
    static Tpl<string, GameObject> createError(string errorFormat, GameObject go) {
      return F.t(string.Format(errorFormat, go), go);
    }

    static string fullPath(GameObject go) {
      return go.transform.parent == null
        ? go.name
        : fullPath(go.transform.parent.gameObject) + "/" + go.name;
    }
  }
}