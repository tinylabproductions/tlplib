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

    [MenuItem("Tools/Show Missing Object References in assets2", false, 57)]
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
              var method = a.GetType().BaseType.GetMethod("RebuildPersistentCallsIfNeeded", BindingFlags.Instance | BindingFlags.NonPublic);
              Debug.LogWarning(method);
              method.Invoke(a, new object[] {});
              var m_PersistentCalls = a.GetType().BaseType.GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
              var m_PersistentCallsValue = m_PersistentCalls.GetValue(a);

              var m_Calls = m_PersistentCallsValue.GetType().GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
              var m_CallsValue = m_PersistentCalls.GetValue(a);

              //int n = (int)m_CallsValue.GetType().GetProperty("Count").GetValue(m_CallsValue, null);
              //for (int i = 0; i < n; i++) {
              //  object[] index = { i };
              //  object myObject = m_CallsValue.GetType().GetProperty("Item", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_CallsValue, index);
              //  Debug.LogWarning(sp.propertyPath);

              //  // Get the object properties  
              //  PropertyInfo[] objectProperties = myObject.GetType().GetProperties();

              //  // Process each property  
              //  foreach (PropertyInfo currentProperty in objectProperties) {
              //    string propertyValue = currentProperty.GetValue(myObject, null).ToString();
              //    Console.WriteLine(propertyValue);
              //  }
              //}

              PropertyInfo listProperty = m_Calls.GetType().GetProperty("List", BindingFlags.Public);
              IEnumerable listObject = (IEnumerable)listProperty.GetValue(m_Calls, null);

              foreach (var persistentCall in (IEnumerable<object>)m_Calls) {
                var isValid = (bool)persistentCall.GetType().GetMethod("IsValid").Invoke(persistentCall, new object[] { });
                if (isValid) {
                  var methodInfo = a.GetType().GetMethod("FindMethod", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(a, new[] { persistentCall });
                  if (methodInfo == null) {
                    Debug.LogWarning(sp.propertyPath);
                  }
                }
              }
              var prop = m_PersistentCallsValue.GetType().GetProperty("Count");
              var count = (int)prop.GetValue(m_PersistentCallsValue, new object[] {});
              Debug.LogWarning(count);
              Debug.LogWarning(a.GetPersistentEventCount());
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