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
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.scenes;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine.Events;
using JetBrains.Annotations;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.validations;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public class ObjectValidator {
    public struct Error {
      public enum Type : byte {
        MissingComponent,
        MissingRequiredComponent,
        MissingReference,
        NullReference,
        EmptyCollection,
        UnityEventInvalidMethod,
        UnityEventInvalid,
        TextFieldBadTag
      }

      public readonly Type type;
      public readonly string message;
      public readonly Object obj;
      public readonly string objFullPath;
      public readonly Option<string> assetPath;

      #region Equality

      public bool Equals(Error other) {
        return type == other.type && string.Equals(message, other.message) && Equals(obj, other.obj) && string.Equals(objFullPath, other.objFullPath) && Equals(assetPath, other.assetPath);
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Error && Equals((Error) obj);
      }

      public override int GetHashCode() {
        unchecked {
          var hashCode = (int) type;
          hashCode = (hashCode * 397) ^ (message != null ? message.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (obj != null ? obj.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (objFullPath != null ? objFullPath.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (assetPath != null ? assetPath.GetHashCode() : 0);
          return hashCode;
        }
      }

      #endregion
      
      public override string ToString() => 
        $"{nameof(Error)}[{type} in '{objFullPath} @ {assetPath.getOrElse("scene obj")}'|{message}]";

      #region Constructors

      public Error(Type type, string message, Object obj) {
        this.type = type;
        this.message = message;
        this.obj = obj;
        objFullPath = fullPath(obj);
        assetPath = AssetDatabase.GetAssetPath(obj).opt();
      }

      // Missing component is null, that is why we need GO
      public static Error missingComponent(GameObject o) => new Error(
        Type.MissingComponent,
        "in GO",
        o
      );

      public static Error emptyCollection(
        Object o, string hierarchy, CheckContext context
      ) => new Error(
        Type.EmptyCollection,
        $"{context}. Property: {hierarchy}",
        o
      );

      public static Error missingReference(
        Object o, string property, CheckContext context
      ) => new Error(
        Type.MissingReference,
        $"{context}. Property: {property}",
        o
      );

      public static Error requiredComponentMissing(
        GameObject go, System.Type requiredType, System.Type requiredBy, CheckContext context
      ) => new Error(
        Type.MissingRequiredComponent,
        $"{context}. {requiredType} missing (required by {requiredBy})",
        go
      );

      public static Error nullReference(
        Object o, string hierarchy, CheckContext context
      ) => new Error(
        Type.NullReference,
        $"{context}. Property: {hierarchy}",
        o
      );

      public static Error unityEventInvalidMethod(
        Object o, string property, int number, CheckContext context
      ) => new Error(
        Type.UnityEventInvalidMethod,
        $"UnityEvent {property} callback number {number} has invalid method in {context}.",
        o
      );

      public static Error unityEventInvalid(
        Object o, string property, int number, CheckContext context
      ) => new Error(
        Type.UnityEventInvalid,
        $"UnityEvent {property} callback number {number} is not valid in {context}.",
        o
      );

      public static Error textFieldBadTag(
        Object o, string hierarchy, CheckContext context
      ) => new Error(
        Type.TextFieldBadTag,
        $"{context}. Property: {hierarchy}",
        o
      );

      #endregion
    }

    public class CheckContext {
      public static readonly CheckContext empty = 
        new CheckContext(Option<string>.None, ImmutableHashSet<Type>.Empty);

      public readonly Option<string> value;
      public readonly ImmutableHashSet<Type> checkedComponentTypes;

      public CheckContext(Option<string> value, ImmutableHashSet<Type> checkedComponentTypes) {
        this.value = value;
        this.checkedComponentTypes = checkedComponentTypes;
      }

      public CheckContext(string value) : this(value.some(), ImmutableHashSet<Type>.Empty) {}

      public override string ToString() => value.getOrElse("unknown ctx");

      public CheckContext withCheckedComponentType(Type c) =>
        new CheckContext(value, checkedComponentTypes.Add(c));
    }

    [UsedImplicitly, MenuItem(
      "TLP/Tools/Validate Objects in Current Scene", 
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
          "Validating Objects", "Please wait...", progress
        ),
        EditorUtility.ClearProgressBar
      );
      showErrors(t._1);
      if (Log.isInfo) Log.info(
        $"{scene.name} {nameof(checkCurrentSceneMenuItem)} finished in {t._2}"
      );
    }

    [UsedImplicitly, MenuItem(
      "TLP/Tools/Validate Selected Objects", 
      isValidateFunction: false, priority: 56
    )]
    static void checkSelectedObjects() {
      var errors = check(
        new CheckContext("Selection"), Selection.objects,
        progress => EditorUtility.DisplayProgressBar(
          "Validating Objects", "Please wait...", progress
        ),
        EditorUtility.ClearProgressBar
      );
      showErrors(errors);
    }

    public static Tpl<ImmutableList<Error>, TimeSpan> checkScene(
      Scene scene, Act<float> onProgress = null, Action onFinish = null
    ) {
      var stopwatch = new Stopwatch().tap(_ => _.Start());
      var objects = getSceneObjects(scene);
      var errors = check(new CheckContext(scene.name), objects, onProgress, onFinish);
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
        new CheckContext("Assets & Deps"), dependencies, onProgress, onFinish
      );
    }

    public static ImmutableList<Error> check(
      CheckContext context, ICollection<Object> objects, 
      Act<float> onProgress = null, Action onFinish = null
    ) {
      var errors = ImmutableList<Error>.Empty;
      var scanned = 0;
      foreach (var o in objects) {
        var progress = (float) scanned++ / objects.Count;
        onProgress?.Invoke(progress);

        var goOpt = F.opt(o as GameObject);
        if (goOpt.isSome) {
          var go = goOpt.get;
          foreach (var transform in go.transform.andAllChildrenRecursive()) {
            var components = transform.GetComponents<Component>();
            foreach (var c in components) {
              errors = 
                c 
                ? errors.AddRange(checkComponent(context, c))
                : errors.Add(Error.missingComponent(transform.gameObject));
            }
          }
        }
        else {
          errors = errors.AddRange(checkComponent(context, o));
        }
      }

      onFinish?.Invoke();

      // FIXME: there should not be a need to a Distinct call here, we have a bug somewhere in the code.
      return errors.Distinct().ToImmutableList();
    }

    public static ImmutableList<Error> checkComponent(CheckContext context, Object component) {
      var errors = ImmutableList<Error>.Empty;

      foreach (var mb in F.opt(component as MonoBehaviour)) {
        var componentType = component.GetType();
        if (!context.checkedComponentTypes.Contains(componentType)) {
          errors = errors.AddRange(checkComponentType(context, mb.gameObject, componentType));
          context = context.withCheckedComponentType(componentType);
        }
      }

      var serObj = new SerializedObject(component);
      var sp = serObj.GetIterator();

      while (sp.NextVisible(enterChildren: true)) {
        if (
          sp.propertyType == SerializedPropertyType.ObjectReference
          && sp.objectReferenceValue == null
          && sp.objectReferenceInstanceIDValue != 0
        ) errors = errors.Add(Error.missingReference(
          component, ObjectNames.NicifyVariableName(sp.name), context
        ));

        if (sp.type == nameof(UnityEvent)) {
          foreach (var evt in getUnityEvent(component, sp.propertyPath)) {
            foreach (var err in checkUnityEvent(evt, component, sp.name, context)) {
              errors = errors.Add(err);
            }
          }
        }
      }

      var fieldErrors = validateFieldsWithAttributes(
        component,
        (err, fieldHierarchy) => {
          switch (err) {
            case FieldAttributeError.NullField:
              return Error.nullReference(component, fieldHierarchy, context);
            case FieldAttributeError.EmptyCollection:
              return Error.emptyCollection(component, fieldHierarchy, context);
            case FieldAttributeError.TextFieldBadTag:
              return Error.textFieldBadTag(component, fieldHierarchy, context);
          }
          return F.matchErr<Error>(nameof(FieldAttributeError), err.ToString());
        }
      );
      errors = errors.AddRange(fieldErrors);

      return errors;
    }

    public static ImmutableList<Error> checkComponentType(
      CheckContext context, GameObject go, Type type
    ) => (
      from rc in type.getAttributes<RequireComponent>(inherit: true)
      from requiredType in new[] {F.opt(rc.m_Type0), F.opt(rc.m_Type1), F.opt(rc.m_Type2)}.flatten()
      where !go.GetComponent(requiredType)
      select requiredType
    ).Aggregate(
      ImmutableList<Error>.Empty, 
      (current, requiredType) => 
        current.Add(Error.requiredComponentMissing(go, requiredType, type, context))
    );

    static Option<Error> checkUnityEvent(
      UnityEventBase evt, Object component, string propertyName, CheckContext context
    ) {
      UnityEventReflector.rebuildPersistentCallsIfNeeded(evt);

      var persistentCalls = evt.__persistentCalls();
      var listPersistentCallOpt = persistentCalls.calls;
      if (listPersistentCallOpt.isNone) return Option<Error>.None;
      var listPersistentCall = listPersistentCallOpt.get;

      var index = 0;
      foreach (var persistentCall in listPersistentCall) {
        index++;

        if (persistentCall.isValid) {
          if (evt.__findMethod(persistentCall).isNone)
            return Error.unityEventInvalidMethod(component, propertyName, index, context).some();
        }
        else
          return Error.unityEventInvalid(component, propertyName, index, context).some();
      }

      return Option<Error>.None;
    }

    static Option<UnityEvent> getUnityEvent(object obj, string fieldName) {
      if (obj == null) return Option<UnityEvent>.None;

      var fiOpt = F.opt(obj.GetType().GetField(fieldName));
      return 
        fiOpt.isSome 
        ? F.opt(fiOpt.get.GetValue(obj) as UnityEvent) 
        : Option<UnityEvent>.None;
    }

    enum FieldAttributeError { NullField, EmptyCollection, TextFieldBadTag }

    static string hierarchyToString(Stack<string> fieldHierarchy) => fieldHierarchy.Reverse().mkString('.');

    static IEnumerable<Error> validateFieldsWithAttributes(
      object o, Fn<FieldAttributeError, string, Error> createError,
      Stack<string> fieldHierarchy = null
    ) {
      fieldHierarchy = fieldHierarchy ?? new Stack<string>();

      var fields = getFilteredFields(o);
      foreach (var fi in fields) {
        fieldHierarchy.Push(fi.Name);
        if (fi.FieldType == typeof(string)) {
          if (fi.getAttributes<TextFieldAttribute>().Any(a => a.Type == TextFieldType.Tag)) {
            var fieldValue = (string)fi.GetValue(o);
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(fieldValue)) {
              yield return createError(FieldAttributeError.TextFieldBadTag, hierarchyToString(fieldHierarchy));
            }
          }
        }
        if (fi.isSerializable()) {
          var fieldValue = fi.GetValue(o);
          var hasNotNull = fi.hasAttribute<NotNullAttribute>();
          // Sometimes we get empty unity object. Equals catches that
          if (fieldValue == null || fieldValue.Equals(null)) {
            if (hasNotNull) yield return createError(FieldAttributeError.NullField, hierarchyToString(fieldHierarchy));
          }
          else {
            var listOpt = F.opt(fieldValue as IList);
            if (listOpt.isSome) {
              var list = listOpt.get;
              if (list.Count == 0 && fi.hasAttribute<NonEmptyAttribute>()) {
                yield return createError(FieldAttributeError.EmptyCollection, hierarchyToString(fieldHierarchy));
              }
              foreach (
                var _err in validateFieldsWithAttributes(list, fi, hasNotNull, fieldHierarchy, createError)
              ) yield return _err;
            }
            else {
              var fieldType = fi.FieldType;
              // Check non-primitive serialized fields.
              if (
                !fieldType.IsPrimitive
                && fieldType.hasAttribute<SerializableAttribute>()
              ) {
                foreach (var _err in validateFieldsWithAttributes(fieldValue, createError, fieldHierarchy))
                  yield return _err;
              }
            }
          }
        }
        fieldHierarchy.Pop();
      }
    }

    static IEnumerable<FieldInfo> getFilteredFields(object o) {
      var fields = o.GetType().getAllFields();
      foreach (var sv in (o as ISkipObjectValidationFields).opt()) {
        var blacklisted = sv.blacklistedFields();
        return fields.Where(fi => !blacklisted.Contains(fi.Name));
      }
      return fields;
    }

    static readonly Type unityObjectType = typeof(Object);

    static IEnumerable<Error> validateFieldsWithAttributes(
      IList list, FieldInfo listFieldInfo, bool hasNotNull, Stack<string> fieldHierarchy, Fn<FieldAttributeError, string, Error> createError
    ) {
      var listItemType = listFieldInfo.FieldType.GetElementType();
      var listItemIsUnityObject = unityObjectType.IsAssignableFrom(listItemType);

      if (listItemIsUnityObject) {
        if (hasNotNull && list.Contains(null)) yield return createError(FieldAttributeError.NullField, hierarchyToString(fieldHierarchy));
      }
      else {
        var index = 0;
        foreach (var listItem in list) {
          fieldHierarchy.Push($"[{index}]");
          foreach (var _err in validateFieldsWithAttributes(listItem, createError, fieldHierarchy))
            yield return _err;
          fieldHierarchy.Pop();
          index++;
        }
      }
    }

    static ImmutableList<Object> getSceneObjects(Scene scene) => 
      scene.GetRootGameObjects()
      .Where(go => go.hideFlags == HideFlags.None)
      .Cast<Object>()
      .ToImmutableList();

    static void showErrors(IEnumerable<Error> errors) {
      foreach (var error in errors)
        if (Log.isError) Log.error(error, error.obj);
    }

    static string fullPath(Object o) {
      var go = o as GameObject;
      return 
        go && go.transform.parent != null 
        ? $"[{fullPath(go.transform.parent.gameObject)}]/{go}"
        : o.ToString();
    }
  }
}