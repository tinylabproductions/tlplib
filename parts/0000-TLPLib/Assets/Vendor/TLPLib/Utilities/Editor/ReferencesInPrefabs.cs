using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine.Events;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class ReferencesInPrefabs : MonoBehaviour {
    static string missingComponent(GameObject go) => 
      $"Missing Component in GO or children: {go}";
    static string missingReference(GameObject go, string component, string property, string context) =>
      $"Missing Ref in: [{context}]{fullPath(go)}. Component: {component}, Property: {property}";
    static string nullReference(GameObject go, string component, string property, string context) =>
      $"Null Ref in: [{context}]{fullPath(go)}. Component: {component}, Property: {property}";
    static string unityEventInvalidMethod(string property, int number) => 
      $"UnityEvent {property} callback number {number} has invalid method";
    static string unityEventNotValid(string property, int number) => 
      $"UnityEvent {property} callback number {number} is not valid";

    static bool anyErrors;
    static readonly Dictionary<Type, IEnumerable<FieldInfo>> typeResultsCache = new Dictionary<Type, IEnumerable<FieldInfo>>();

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
      if (anyErrors) {
        anyErrors = false;
        throw new Exception($"Build failed ({target})");
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
      Debug.Log($"{nameof(findMissingReferencesInCurrentScene)} finished");
    }

    public static void missingReferencesInAssets() {
      //var loadedScenes = scenes.Select(s => AssetDatabase.LoadMainAssetAtPath(scene)).ToArray();
      var depsOfScene = EditorUtility.CollectDependencies(new UnityEngine.Object[] {}).Where( x => x is GameObject || x is ScriptableObject);
    }

    public static List<Tpl<string, GameObject>> findMissingReferences(string context, GameObject[] objects, bool useProgress = true) {
      var errors = new List<Tpl<string, GameObject>>();
      var scanned = 0;
      foreach (var go in objects) {
        if (useProgress) {
          EditorUtility.DisplayProgressBar($"{nameof(findMissingReferences)}", $"{nameof(findMissingReferences)}", (float)scanned++ / objects.Length);
        }
        var components = go.GetComponentsInChildren<Component>();

        foreach (var c in components) {
          if (!c) {
            errors.Add(createError(missingComponent(c.gameObject), c.gameObject));
            continue;
          }

          var so = new SerializedObject(c);
          var sp = so.GetIterator();

          while (sp.NextVisible(true)) {
            if (sp.propertyType == SerializedPropertyType.ObjectReference) {
              if (sp.objectReferenceValue == null
                  && sp.objectReferenceInstanceIDValue != 0) {
                errors.Add(createError(missingReference(c.gameObject, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name), context), go));
              }
            }

            #region Unity Events
            if (sp.type == nameof(UnityEvent)) {
              var uniEvent = getUnityEvent(c, sp.propertyPath);
              var baseType = uniEvent?.GetType().BaseType;
              var method = baseType?.GetMethod("RebuildPersistentCallsIfNeeded", BindingFlags.Instance | BindingFlags.NonPublic);
              method?.Invoke(uniEvent, new object[] { });

              var m_PersistentCalls = baseType?.GetField("m_PersistentCalls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
              var m_PersistentCallsValue = m_PersistentCalls?.GetValue(uniEvent);

              var m_Calls = m_PersistentCallsValue?.GetType().GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
              var m_CallsValue = m_PersistentCalls?.GetValue(uniEvent);

              var methods = baseType?.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
              var methodInfo = methods?.First(mi => mi.Name.Equals("FindMethod") && mi.GetParameters().Any());

              var listObject = (IEnumerable)m_Calls?.GetValue(m_CallsValue);
              var index = 0;
              if (listObject != null)
                foreach (var persistentCall in listObject) {
                  index++;
                  Func<string, int, string> format = null;

                  var isValid = (bool)persistentCall.GetType().GetMethod("IsValid").Invoke(persistentCall, new object[] { });
                  if (isValid) {
                    var mi = methodInfo?.Invoke(uniEvent, new[] { persistentCall });
                    if (mi == null) format = unityEventInvalidMethod;
                  }
                  else format = unityEventNotValid;

                  if (format != null)
                    errors.Add(createError(format(ObjectNames.NicifyVariableName(sp.name), index), c.gameObject));
                }
            }
            #endregion
          }

          var notNullFields = ReferencesInPrefabs.notNullFields(c.GetType());

          foreach (var field in notNullFields) {
            if (!(field.GetValue(c) is UnityEngine.Object)) {
              errors.Add(createError(nullReference(c.gameObject, c.GetType().Name, field.Name, context), go));
            }
          }
        }
      }

      if (useProgress) EditorUtility.ClearProgressBar();
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
      if (typeResultsCache.ContainsKey(type)) {
        IEnumerable<FieldInfo> result;
        typeResultsCache.TryGetValue(type, out result); 
        return result;
      }

      List<FieldInfo> results;
      var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (type.hasAttribute<NotNullAttribute>()) {
        results = fields.Where(fi => !fi.hasAttribute<CanBeNullAttribute>()).ToList();
      } else {
        results = fields.Where(fi => fi.hasAttribute<NotNullAttribute>()).ToList();
      }
      
      typeResultsCache.Add(type, results);
      return results;
    }

    static GameObject[] getSceneObjects() {
      return SceneManager.GetActiveScene().GetRootGameObjects()
        .Where(go => go.hideFlags == HideFlags.None).ToArray();
    }

    static void showError(Tpl<string, GameObject> error) { Debug.LogError(error._1, error._2); }

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