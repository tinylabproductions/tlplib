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
    /// <param name="containingObject">Unity object </param>
    /// <param name="obj">Object that is being validated.</param>
    /// <returns></returns>
    public delegate IEnumerable<ErrorMsg> CustomObjectValidator(Object containingObject, object obj);

    interface IErrorFactory {
      Error nullField(FieldHierarchyStr hierarchy);
      Error emptyCollection(FieldHierarchyStr hierarchy);
      Error badTextFieldTag(FieldHierarchyStr hierarchy);
      Error custom(FieldHierarchyStr hierarchy, ErrorMsg customErrorMessage);
    }

    public struct Error : IEquatable<Error> {
      public enum Type : byte {
        MissingComponent,
        MissingRequiredComponent,
        MissingReference,
        NullReference,
        EmptyCollection,
        UnityEventInvalidMethod,
        UnityEventInvalid,
        TextFieldBadTag,
        CustomValidation
      }

      public readonly Type type;
      public readonly string message;
      public readonly Object obj;
      public readonly string objFullPath;
      public readonly Either<AssetPath, ScenePath> location;

      #region Equality

      public bool Equals(Error other) {
        return type == other.type && string.Equals(message, other.message) && Equals(obj, other.obj) && string.Equals(objFullPath, other.objFullPath) && location.Equals(other.location);
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
          hashCode = (hashCode * 397) ^ location.GetHashCode();
          return hashCode;
        }
      }

      public static bool operator ==(Error left, Error right) { return left.Equals(right); }
      public static bool operator !=(Error left, Error right) { return !left.Equals(right); }

      #endregion
      
      public override string ToString() => 
        $"{nameof(Error)}[" +
        $"{type} " +
        $"in '{objFullPath}' " +
        $"@ '{location.fold(asset => asset.path, scenePath => scenePath.path)}'. " +
        $"{message}" +
        $"]";

      #region Constructors

      public Error(Type type, string message, Object obj) {
        this.type = type;
        this.message = message;
        this.obj = obj;
        objFullPath = fullPath(obj);

        location = (
             lookupAssetPath(obj).map(_ => _.left().r<ScenePath>())
          || lookupScenePath(obj).map(_ => _.right().l<AssetPath>())
        ).getOrThrow(() => new Exception(
          $"Unable to find a location of '{obj}'! It's neither an asset, " +
          $"nor in a scene. Where is it? I don't know. But this sure as heck " +
          $"needs to be fixed."
        ));
      }

      static Option<AssetPath> lookupAssetPath(Object o) =>
        AssetDatabase.GetAssetPath(o).nonEmptyOpt().map(_ => new AssetPath(_));

      static Option<ScenePath> lookupScenePath(Object o) =>
        from go in F.opt(o as GameObject) || F.opt(o as Component).map(c => c.gameObject)
        from path in go.scene.path.nonEmptyOpt(trim: true)
        select new ScenePath(path);

      // Missing component is null, that is why we need GO
      public static Error missingComponent(GameObject o) => new Error(
        Type.MissingComponent,
        "in GO",
        o
      );

      public static Error emptyCollection(
        Object o, FieldHierarchyStr hierarchy, CheckContext context
      ) => new Error(
        Type.EmptyCollection,
        $"{context}. Property: {hierarchy.s}",
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
        Object o, FieldHierarchyStr hierarchy, CheckContext context
      ) => new Error(
        Type.NullReference,
        $"{context}. Property: {hierarchy.s}",
        o
      );

      static string unityEventMessagePrefix(string property, int index) =>
        $"In property '{property}' callback at index {index} of UnityEvent";
      static string unityEventMessageSuffix(CheckContext context) =>
        $"in context '{context}'.";

      public static Error unityEventInvalidMethod(
        Object o, string property, int index, CheckContext context
      ) => new Error(
        Type.UnityEventInvalidMethod,
        $"{unityEventMessagePrefix(property, index)} has invalid method " +
          unityEventMessageSuffix(context),
        o
      );

      public static Error unityEventInvalid(
        Object o, string property, int index, CheckContext context
      ) => new Error(
        Type.UnityEventInvalid,
        $"{unityEventMessagePrefix(property, index)} is not valid " + 
          unityEventMessageSuffix(context),
        o
      );

      public static Error badTextFieldTag(
        Object o, FieldHierarchyStr hierarchy, CheckContext context
      ) => new Error(
        Type.TextFieldBadTag,
        $"{context}. Property: {hierarchy.s}",
        o
      );

      public static Error customError(
        Object o, FieldHierarchyStr hierarchy, ErrorMsg error, CheckContext context  
      ) => new Error(
        Type.CustomValidation,
        $"{context}. Property: {hierarchy.s}. Error: {error}",
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
        Option<CustomObjectValidator>.None,
        progress => EditorUtility.DisplayProgressBar(
          "Validating Objects", "Please wait...", progress
        ),
        EditorUtility.ClearProgressBar
      );
      showErrors(t._1);
      if (Log.d.isInfo()) Log.d.info(
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
        Option<CustomObjectValidator>.None,
        progress => EditorUtility.DisplayProgressBar(
          "Validating Objects", "Please wait...", progress
        ),
        EditorUtility.ClearProgressBar
      );
      showErrors(errors);
    }

    public static Tpl<ImmutableList<Error>, TimeSpan> checkScene(
      Scene scene, Option<CustomObjectValidator> customValidatorOpt, 
      Act<float> onProgress = null, Action onFinish = null
    ) {
      var stopwatch = new Stopwatch().tap(_ => _.Start());
      var objects = getSceneObjects(scene);
      var errors = check(new CheckContext(scene.name), objects, customValidatorOpt, onProgress, onFinish);
      return F.t(errors, stopwatch.Elapsed);
    }

    public static ImmutableList<Error> checkAssetsAndDependencies(
      IEnumerable<PathStr> assets, Option<CustomObjectValidator> customValidatorOpt,
      Act<float> onProgress = null, Action onFinish = null
    ) {
      var loadedAssets = 
        assets.Select(s => AssetDatabase.LoadMainAssetAtPath(s)).ToArray();
      var dependencies = 
        EditorUtility.CollectDependencies(loadedAssets)
        .Where(x => x is GameObject || x is ScriptableObject)
        .ToImmutableList();
      return check(
        // and instead of &, because unity does not show '&' in some windows
        new CheckContext("Assets and Deps"),
        dependencies, customValidatorOpt, onProgress, onFinish
      );
    }

    public static ImmutableList<Error> check(
      CheckContext context, ICollection<Object> objects, 
      Option<CustomObjectValidator> customValidatorOpt = default(Option<CustomObjectValidator>),
      Act<float> onProgress = null, Action onFinish = null
    ) {
      Option.ensureValue(ref customValidatorOpt);

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
                ? errors.AddRange(checkComponent(context, c, customValidatorOpt))
                : errors.Add(Error.missingComponent(transform.gameObject));
            }
          }
        }
        else {
          errors = errors.AddRange(checkComponent(context, o, customValidatorOpt));
        }
      }

      onFinish?.Invoke();

      // FIXME: there should not be a need to a Distinct call here, we have a bug somewhere in the code.
      return errors.Distinct().ToImmutableList();
    }

    public static ImmutableList<Error> checkComponent(
      CheckContext context, Object component, Option<CustomObjectValidator> customObjectValidatorOpt
    ) {
      var errors = ImmutableList<Error>.Empty;

      foreach (var mb in F.opt(value: component as MonoBehaviour)) {
        var componentType = component.GetType();
        if (!context.checkedComponentTypes.Contains(item: componentType)) {
          errors = errors.AddRange(items: checkComponentType(context: context, go: mb.gameObject, type: componentType));
          context = context.withCheckedComponentType(c: componentType);
        }
      }

      var serObj = new SerializedObject(obj: component);
      var sp = serObj.GetIterator();

      while (sp.NextVisible(enterChildren: true)) {
        if (
          sp.propertyType == SerializedPropertyType.ObjectReference
          && sp.objectReferenceValue == null
          && sp.objectReferenceInstanceIDValue != 0
        ) errors = errors.Add(value: Error.missingReference(
          o: component, property: sp.name, context: context
        ));

        if (sp.type == nameof(UnityEvent)) {
          var evt = getUnityEvent(obj: component, fieldName: sp.propertyPath).getOrThrow(() => 
            new Exception(
              $"There should have been a {nameof(UnityEvent)} in property '{sp.name}' " +
              $"on '{component}' @ '{AssetDatabase.GetAssetPath(component)}', " +
              $"but we could not find it! This seems like a programmer error!"
            )
          );
          errors = errors.AddRange(checkUnityEvent(
            evt: evt, component: component, propertyName: sp.name, context: context
          ).asEnum());
        }
      }

      var fieldErrors = validateFields(
        containingComponent: component,
        objectBeingValidated: component,
        createError: new ErrorFactory(component, context), 
        customObjectValidatorOpt: customObjectValidatorOpt
      );
      errors = errors.AddRange(items: fieldErrors);

      return errors;
    }

    class ErrorFactory : IErrorFactory {
      readonly Object component;
      readonly CheckContext context;

      public ErrorFactory(Object component, CheckContext context) {
        this.component = component;
        this.context = context;
      }

      public Error nullField(FieldHierarchyStr hierarchy) =>
        Error.nullReference(o: component, hierarchy: hierarchy, context: context);

      public Error emptyCollection(FieldHierarchyStr hierarchy) =>
        Error.emptyCollection(o: component, hierarchy: hierarchy, context: context);

      public Error badTextFieldTag(FieldHierarchyStr hierarchy) =>
        Error.badTextFieldTag(o: component, hierarchy: hierarchy, context: context);

      public Error custom(FieldHierarchyStr hierarchy, ErrorMsg customErrorMessage) =>
        Error.customError(o: component, hierarchy: hierarchy, error: customErrorMessage, context: context);
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
        if (persistentCall.isValid) {
          if (evt.__findMethod(persistentCall).isNone)
            return Error.unityEventInvalidMethod(component, propertyName, index, context).some();
        }
        else
          return Error.unityEventInvalid(component, propertyName, index, context).some();

        index++;
      }

      return Option<Error>.None;
    }

    static Option<UnityEvent> getUnityEvent(object obj, string fieldName) {
      if (obj == null) return Option<UnityEvent>.None;

      var fiOpt = obj.GetType().GetFieldInHierarchy(fieldName);
      return 
        fiOpt.isSome 
        ? F.opt(fiOpt.__unsafeGetValue.GetValue(obj) as UnityEvent) 
        : Option<UnityEvent>.None;
    }

    public struct FieldHierarchyStr {
      public readonly string s;
      public FieldHierarchyStr(string s) { this.s = s; }
      public override string ToString() => $"{nameof(FieldHierarchy)}({s})";
    }

    class FieldHierarchy {
      public readonly Stack<string> stack = new Stack<string>();

      public FieldHierarchyStr asString() => new FieldHierarchyStr(stack.Reverse().mkString('.'));
    }

    static IEnumerable<Error> validateFields(
      Object containingComponent, 
      object objectBeingValidated,
      IErrorFactory createError, 
      Option<CustomObjectValidator> customObjectValidatorOpt,
      FieldHierarchy fieldHierarchy = null
    ) {
      fieldHierarchy = fieldHierarchy ?? new FieldHierarchy();

      foreach (var onObjectValidatable in F.opt(objectBeingValidated as OnObjectValidate)) {
        foreach (var error in onObjectValidatable.onObjectValidate(containingComponent)) {
          yield return createError.custom(fieldHierarchy.asString(), error);
        }
      }

      foreach (var customValidator in customObjectValidatorOpt) {
        foreach (var _err in customValidator(containingComponent, objectBeingValidated)) {
          yield return createError.custom(fieldHierarchy.asString(), _err);
        }
      }

      var fields = getFilteredFields(objectBeingValidated);
      foreach (var fi in fields) {
        fieldHierarchy.stack.Push(fi.Name);
        if (fi.FieldType == typeof(string)) {
          if (fi.getAttributes<TextFieldAttribute>().Any(a => a.Type == TextFieldType.Tag)) {
            var fieldValue = (string)fi.GetValue(objectBeingValidated);
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(fieldValue)) {
              yield return createError.badTextFieldTag(fieldHierarchy.asString());
            }
          }
        }
        if (fi.isSerializable()) {
          var fieldValue = fi.GetValue(objectBeingValidated);
          var hasNotNull = fi.hasAttribute<NotNullAttribute>();
          // Sometimes we get empty unity object. Equals catches that
          if (fieldValue == null || fieldValue.Equals(null)) {
            if (hasNotNull) yield return createError.nullField(fieldHierarchy.asString());
          }
          else {
            var listOpt = F.opt(fieldValue as IList);
            if (listOpt.isSome) {
              var list = listOpt.get;
              if (list.Count == 0 && fi.hasAttribute<NonEmptyAttribute>()) {
                yield return createError.emptyCollection(fieldHierarchy.asString());
              }
              var fieldValidationResults = validateFields(
                containingComponent, list, fi, hasNotNull, 
                fieldHierarchy, createError, customObjectValidatorOpt
              );
              foreach (var _err in fieldValidationResults) yield return _err;
            }
            else {
              var fieldType = fi.FieldType;
              // Check non-primitive serialized fields.
              if (
                !fieldType.IsPrimitive
                && fieldType.hasAttribute<SerializableAttribute>()
              ) {
                var validationErrors = validateFields(
                  containingComponent, fieldValue, createError,
                  customObjectValidatorOpt, fieldHierarchy
                );
                foreach (var _err in validationErrors) yield return _err;
              }
            }
          }
        }
        fieldHierarchy.stack.Pop();
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

    static IEnumerable<Error> validateFields(
      Object containingComponent, IList list, FieldInfo listFieldInfo, 
      bool hasNotNull, FieldHierarchy fieldHierarchy,
      IErrorFactory createError,
      Option<CustomObjectValidator> customObjectValidatorOpt 
    ) {
      var listItemType = listFieldInfo.FieldType.GetElementType();
      var listItemIsUnityObject = unityObjectType.IsAssignableFrom(listItemType);

      if (listItemIsUnityObject) {
        if (hasNotNull && list.Contains(null))
          yield return createError.nullField(fieldHierarchy.asString());
      }
      else {
        var index = 0;
        foreach (var listItem in list) {
          fieldHierarchy.stack.Push($"[{index}]");
          var validationResults = validateFields(
            containingComponent, listItem, createError, 
            customObjectValidatorOpt, fieldHierarchy
          );
          foreach (var _err in validationResults) yield return _err;
          fieldHierarchy.stack.Pop();
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
        Log.d.error(error.ToString(), context: error.obj);
    }

    static string fullPath(Object o) {
      var go = o as GameObject;
      return 
        go && go.transform.parent != null 
        ? $"[{fullPath(go.transform.parent.gameObject)}]/{go}"
        : o.ToString();
    }
  }

  public static class CustomObjectValidatorExts {
    public static ObjectValidator.CustomObjectValidator join(
      this ObjectValidator.CustomObjectValidator a, ObjectValidator.CustomObjectValidator b
    ) => (containingObject, obj) => a(containingObject, obj).Concat(b(containingObject, obj));
  }
}
