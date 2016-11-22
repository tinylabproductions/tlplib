using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine.Events;
using JetBrains.Annotations;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class MissingReferenceFinder {
    public struct Error {
      public enum Type {
        MissingComponent, MissingReference, NullReference, UnityEventInvalidMethod, UnityEventInvalid
      }

      public readonly Type type;
      public readonly string message;
      public readonly Object context;

      public override string ToString() => 
        $"{nameof(Error)}[{type}, {nameof(context)}: {context}; {message}]";

      #region Constructors

      public Error(Type type, string message, Object context) {
        this.type = type;
        this.message = message;
        this.context = context;
      }

      public static Error missingComponent(Object o) => new Error(
        Type.MissingComponent,
        $"Missing Component in GO or children: {o}",
        o
      );

      public static Error missingReference(
        Object o, string component, string property, string context
      ) => new Error(
        Type.MissingReference,
        $"Missing Ref in: [{context}]{fullPath(o)}. Component: {component}, Property: {property}",
        o
      );

      public static Error nullReference(
        Object o, string component, string property, string context
      ) => new Error(
        Type.NullReference,
        $"Null Ref in: [{context}]{fullPath(o)}. Component: {component}, Property: {property}",
        o
      );

      public static Error unityEventInvalidMethod(
        Object o, string property, int number, string context
      ) => new Error(
        Type.UnityEventInvalidMethod,
        $"UnityEvent {property} callback number {number} has invalid method in [{context}]{fullPath(o)}.",
        o
      );

      public static Error unityEventNotValid(
        Object o, string property, int number, string context
      ) => new Error(
        Type.UnityEventInvalid,
        $"UnityEvent {property} callback number {number} is not valid in [{context}]{fullPath(o)}.",
        o
      );

      #endregion
    }

    [MenuItem(
      "Tools/Show Missing Object References in Current Scene", 
      isValidateFunction: false, priority: 55
    )]
    static void checkCurrentSceneMenuItem() {
      if (EditorApplication.isPlayingOrWillChangePlaymode) {
        EditorUtility.DisplayDialog(
          "In Play Mode!",
          "This action cannot be run in play mode. Aborting!",
          "OK"
        );
        return;
      }

      var scene = SceneManager.GetActiveScene();
      var t = checkScene(
        scene,
        progress => EditorUtility.DisplayProgressBar(
          "Checking Missing References", "Please wait...", progress
        ),
        EditorUtility.ClearProgressBar
      );
      showErrors(t._1);
      Debug.LogWarning(
        $"{scene.name} {nameof(checkCurrentSceneMenuItem)} finished in {t._2}"
      );
    }

    public static Tpl<ImmutableList<Error>, TimeSpan> checkScene(
      Scene scene, Act<float> onProgress = null, Action onFinish = null
    ) {
      var stopwatch = new Stopwatch().tap(_ => _.Start());
      var objects = getSceneObjects(scene);
      var errors = check(scene.name, objects, onProgress, onFinish);
      return F.t(errors, stopwatch.Elapsed);
    }

    public static ImmutableList<Error> checkAssetsAndDependencies(
      IEnumerable<PathStr> assets, Act<float> onProgress = null, Action onFinish = null
    ) {
      var loadedAssets = 
        assets.Select(s => AssetDatabase.LoadMainAssetAtPath(s)).ToArray();
      var dependencies = 
        EditorUtility.CollectDependencies(loadedAssets)
        .Where(x => x is GameObject || x is ScriptableObject)
        .ToImmutableList();
      return check(
        nameof(checkAssetsAndDependencies), dependencies, onProgress, onFinish
      );
    }

    public static ImmutableList<Error> check(
      string context, ICollection<Object> objects, 
      Act<float> onProgress = null, Action onFinish = null
    ) {
      var errors = ImmutableList<Error>.Empty;
      var scanned = 0;
      foreach (var o in objects) {
        var progress = (float) scanned++ / objects.Count;
        onProgress?.Invoke(progress);

        var goOpt = F.opt(o as GameObject);
        if (goOpt.isDefined) {
          var components = goOpt.get.GetComponentsInChildren<Component>();
          foreach (var c in components) {
            errors = 
              c 
              ? errors.AddRange(checkComponent(context, c))
              : errors.Add(Error.missingComponent(c));
          }
        }
        else {
          errors = errors.AddRange(checkComponent(context, o));
        }
      }

      onFinish?.Invoke();

      return errors;
    }

    public static ImmutableList<Error> checkComponent(string context, Object component) {
      var errors = ImmutableList<Error>.Empty;

      var serObj = new SerializedObject(component);
      var sp = serObj.GetIterator();

      while (sp.NextVisible(enterChildren: true)) {
        if (
          sp.propertyType == SerializedPropertyType.ObjectReference
          && sp.objectReferenceValue == null
          && sp.objectReferenceInstanceIDValue != 0
        ) errors = errors.Add(Error.missingReference(
          component, component.GetType().Name, 
          ObjectNames.NicifyVariableName(sp.name), ""
        ));

        if (sp.type == nameof(UnityEvent)) {
          foreach (var evt in getUnityEvent(component, sp.propertyPath)) {
            foreach (var err in checkUnityEvent(evt, component, sp.name, context)) {
              errors = errors.Add(err);
            }
          }
        }
      }

      var nullFields = lookupFieldsWithNotNullThatAreNull(component);
      errors = errors.AddRange(nullFields.Select(field => 
        Error.nullReference(component, component.GetType().Name, field.Name, context)
      ));

      return errors;
    }

    static Option<Error> checkUnityEvent(
      UnityEventBase evt, Object component, string propertyName, string context
    ) {
      UnityEventReflector.rebuildPersistentCallsIfNeeded(evt);

      var persistentCalls = evt.__persistentCalls();
      var listPersistentCallOpt = persistentCalls.calls;
      if (listPersistentCallOpt.isEmpty) return Option<Error>.None;
      var listPersistentCall = listPersistentCallOpt.get;

      var index = 0;
      foreach (var persistentCall in listPersistentCall) {
        index++;

        if (persistentCall.isValid) {
          if (evt.__findMethod(persistentCall).isEmpty)
            return Error.unityEventInvalidMethod(component, propertyName, index, context).some();
        }
        else
          return Error.unityEventNotValid(component, propertyName, index, context).some();
      }

      return Option<Error>.None;
    }

    static Option<UnityEvent> getUnityEvent(object obj, string fieldName) {
      if (obj == null) return Option<UnityEvent>.None;

      var fiOpt = F.opt(obj.GetType().GetField(fieldName));
      return 
        fiOpt.isDefined 
        ? F.opt(fiOpt.get.GetValue(obj) as UnityEvent) 
        : Option<UnityEvent>.None;
    }

    static IEnumerable<FieldInfo> lookupFieldsWithNotNullThatAreNull(object o) {
      var type = o.GetType();
      var fields = type.GetFields(
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
      );
      foreach (var fi in fields) {
        if (
          (fi.IsPublic && !fi.hasAttribute<NonSerializedAttribute>())
          || (fi.IsPrivate && fi.hasAttribute<SerializeField>())
        ) {
          var fieldValue = fi.GetValue(o);
          var hasNotNull = fi.hasAttribute<NotNullAttribute>();
          if (fieldValue == null) {
            if (hasNotNull) yield return fi;
          }
          else {
            var listOpt = F.opt(fieldValue as IList);
            if (listOpt.isDefined) {
              foreach (
                var _fi in lookupFieldsWithNotNullThatAreNull(listOpt.get, fi, hasNotNull)
              ) yield return _fi;
            }
            else {
              var fieldType = fi.FieldType;
              // Check non-primitive serialized fields.
              if (
                !fieldType.IsPrimitive
                && fieldType.hasAttribute<SerializableAttribute>()
              ) {
                foreach (var _fi in lookupFieldsWithNotNullThatAreNull(fieldValue))
                  yield return _fi;
              }
            }
          }
        }
      }
    }

    static readonly Type unityObjectType = typeof(Object);

    static IEnumerable<FieldInfo> lookupFieldsWithNotNullThatAreNull(
      IList list, FieldInfo listFieldInfo, bool hasNotNull
    ) {
      var listItemType = listFieldInfo.FieldType.GetElementType();
      var listItemIsUnityObject = unityObjectType.IsAssignableFrom(listItemType);

      if (listItemIsUnityObject) {
        if (hasNotNull && list.Contains(null)) yield return listFieldInfo;
      }
      else {
        foreach (var listItem in list)
          foreach (var _fi in lookupFieldsWithNotNullThatAreNull(listItem))
            yield return _fi;
      }
    }

    static ImmutableList<Object> getSceneObjects(Scene scene) => 
      scene.GetRootGameObjects()
      .Where(go => go.hideFlags == HideFlags.None)
      .Cast<Object>()
      .ToImmutableList();

    static void showErrors(IEnumerable<Error> errors) {
      foreach (var error in errors) Debug.LogError(error.message, error.context);
    }

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