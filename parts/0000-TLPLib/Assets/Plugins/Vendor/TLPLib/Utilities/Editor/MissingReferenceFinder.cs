using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine.Events;
using JetBrains.Annotations;
using com.tinylabproductions.TLPLib.Filesystem;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class MissingReferenceFinder : MonoBehaviour {
    public enum ErrorType {MISSING_COMP, MISSING_REF, NULL_REF, UE_INVALID_METHOD, UE_NOT_VALID}

    public struct ReferenceError {
      public readonly ErrorType errorType;
      public readonly Tpl<string, Object> message;

      public ReferenceError(ErrorType errorType, Tpl<string, Object> message) {
        this.errorType = errorType;
        this.message = message;
      }
    }

    static ReferenceError missingComponent(Object o) => new ReferenceError(
      ErrorType.MISSING_COMP,
      new Tpl<string, Object>($"Missing Component in GO or children: {o}", o)
    );
    static ReferenceError missingReference(Object o, string component, string property, string context) => new ReferenceError(
      ErrorType.MISSING_REF,
      new Tpl<string, Object>($"Missing Ref in: [{context}]{fullPath(o)}. Component: {component}, Property: {property}", o)
    );
    static ReferenceError nullReference(Object o, string component, string property, string context) => new ReferenceError(
      ErrorType.NULL_REF,
      new Tpl<string, Object>($"Null Ref in: [{context}]{fullPath(o)}. Component: {component}, Property: {property}", o)
    );
    static ReferenceError unityEventInvalidMethod(Object o, string property, int number, string context) => new ReferenceError(
      ErrorType.UE_INVALID_METHOD,
      new Tpl<string, Object>($"UnityEvent {property} callback number {number} has invalid method in [{context}]{fullPath(o)}.", o)
    );
    static ReferenceError unityEventNotValid(Object o, string property, int number, string context) => new ReferenceError(
      ErrorType.UE_NOT_VALID,
      new Tpl<string, Object>($"UnityEvent {property} callback number {number} is not valid in [{context}]{fullPath(o)}.", o)
    );

    static bool anyErrors;

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
      if (!anyErrors) return;

      anyErrors = false;
      throw new Exception($"Build failed ({target})");
    }

    [PostProcessScene(0)]
    [MenuItem("Tools/Show Missing Object References in scene", false, 55)]
    public static void findMissingReferencesInCurrentScene() {
      var objects = EditorApplication.isPlayingOrWillChangePlaymode
        ? Resources.FindObjectsOfTypeAll<Object>().Where(o => !AssetDatabase.Contains(o)).ToArray()
        : getSceneObjects();
      showErrors(findMissingReferences(SceneManager.GetActiveScene().name, objects, true));
      Debug.Log($"{nameof(findMissingReferencesInCurrentScene)} finished");
    }

    public static ImmutableList<string> missingReferencesInAssets(IEnumerable<PathStr> scenes) {
      var errors = new List<ReferenceError>();
      scenes.Select(s => AssetDatabase.LoadMainAssetAtPath(s)).ToList().ForEach(scene => {
        var depsOfScene = EditorUtility.CollectDependencies(new []{ scene }).Where(x => x is GameObject || x is ScriptableObject);
        errors.AddRange(findMissingReferences(scene.name, depsOfScene.ToArray()));
      });

      showErrors(errors);
      return errors.Select(x => x.message._1).ToImmutableList();
    }

    public static List<ReferenceError> findMissingReferences(string context, Object[] objects, bool useProgress = false) {
      var errors = new List<ReferenceError>();
      var scanned = 0;
      foreach (var o in objects) {
        if (useProgress) {
          var methodName = $"{nameof(findMissingReferences)}";
          EditorUtility.DisplayProgressBar(methodName, methodName, (float)scanned++ / objects.Length);
        }

        var go = o as GameObject;
        if (go) {
          var components = go.GetComponentsInChildren<Component>();
          foreach (var c in components) {
            errors.AddRange(checkComponent(context, c));
            if (c) continue;

            errors.Add(missingComponent(c.gameObject));
          }
        } else {
          errors.AddRange(checkComponent(context, o));
        }
      }

      if (useProgress) EditorUtility.ClearProgressBar();
      return errors;
    }

    public static List<ReferenceError> checkComponent(string context, Object component) {
      var errors = new List<ReferenceError>();

      var serObj = new SerializedObject(component);
      var sp = serObj.GetIterator();

      while (sp.NextVisible(true)) {
        if (sp.propertyType == SerializedPropertyType.ObjectReference) {
          if (sp.objectReferenceValue == null
              && sp.objectReferenceInstanceIDValue != 0) {
            errors.Add(missingReference(component, component.GetType().Name, ObjectNames.NicifyVariableName(sp.name), ""));
          }
        }

        #region Unity Events
        if (sp.type != nameof(UnityEvent)) continue;

        var uniEvent = getUnityEvent(component, sp.propertyPath);
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
        if (listObject == null) continue;

        var index = 0;
        foreach (var persistentCall in listObject) {
          index++;
          Fn<Object, string, int, string, ReferenceError> err = null;

          var isValid = (bool)persistentCall.GetType().GetMethod("IsValid").Invoke(persistentCall, new object[] { });
          if (isValid) {
            var mi = methodInfo?.Invoke(uniEvent, new[] { persistentCall });
            if (mi == null) err = unityEventInvalidMethod;
          }
          else err = unityEventNotValid;

          if (err != null)
            errors.Add(err(component, ObjectNames.NicifyVariableName(sp.name), index, context));
        }

        #endregion
      }

      var notNullFields = MissingReferenceFinder.notNullFields(component);
      errors.AddRange(notNullFields.Select(field =>
        nullReference(component, component.GetType().Name, field.Name, context)
      ));

      return errors;
    }

    static UnityEvent getUnityEvent(object obj, string fieldName) {
      if (obj == null) return null;

      var fi = obj.GetType().GetField(fieldName);
      if (fi != null) return fi.GetValue(obj) as UnityEvent;

      return null;
    }

    static IEnumerable<FieldInfo> notNullFields(object o) {
      var type = o.GetType();
      var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      var results = new List<FieldInfo>();
      foreach (var fi in fields) {
        if (fi.IsPublic && !fi.hasAttribute<NonSerializedAttribute>() || (fi.IsPrivate && fi.hasAttribute<SerializeField>())) {

          if (fi.hasAttribute<NotNullAttribute>()) {
            if (!(fi.GetValue(o) is Object))
              results.Add(fi);
          }

          var fieldType = fi.FieldType;
          if (fieldType.hasAttribute<SerializableAttribute>() && !fieldType.IsPrimitive) {
            var fieldValue = fi.GetValue(o);
            if (fieldValue != null)
              results.AddRange(notNullFields(fieldValue));
          }
        }
      }
      
      return results;
    }

    static GameObject[] getSceneObjects() {
      return SceneManager.GetActiveScene().GetRootGameObjects()
        .Where(go => go.hideFlags == HideFlags.None).ToArray();
    }

    static void showErrors(List<ReferenceError> errors) {
      foreach (var error in errors) showError(error.message);
      anyErrors = errors.Any();
    }

    static void showError(Tpl<string, Object> error) { Debug.LogError(error._1, error._2); }

    static string fullPath(Object o) {
      var go = o as GameObject;
      if (go)  
        return go.transform.parent == null
          ? go.name
          : fullPath(go.transform.parent.gameObject) + "/" + go.name;

      return o.name;
    }
  }
}