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
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.validations;
using pzd.lib.exts;
using pzd.lib.functional;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    public static readonly Action<Progress> DEFAULT_ON_PROGRESS = progress => EditorUtility.DisplayProgressBar(
      "Validating Objects", "Please wait...", progress.ratio
    );
    public static readonly Action DEFAULT_ON_FINISH = EditorUtility.ClearProgressBar;
    
    #region Menu Items
    
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
      var t = checkSceneWithTime(scene, None._, DEFAULT_ON_PROGRESS, DEFAULT_ON_FINISH);
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
        None._,
        progress => EditorUtility.DisplayProgressBar(
          "Validating Objects", "Please wait...", progress.ratio
        ),
        EditorUtility.ClearProgressBar, UniqueValuesCache.create.some()
      );
      showErrors(errors);
    }
    
    #endregion
    
    [PublicAPI]
    public static void showErrors(IEnumerable<Error> errors, Log.Level level = Log.Level.ERROR) {
      var log = Log.d;
      if (log.willLog(level))
        foreach (var error in errors) {
          // If context is a MonoBehaviour,
          // then unity does not ping the object (only a folder) when clicked on the log message.
          // But it works fine for GameObjects
          var maybeComponent = error.obj as Component;
          var hasGameObject = maybeComponent && maybeComponent.gameObject;
          var context = hasGameObject ? getRootGO(maybeComponent) : error.obj;
          log.log(level, LogEntry.simple(error.ToString(), context: context));
          
          static GameObject getRootGO(Component component) {
            var t = component.transform;
            while (true) {
              if (t.parent == null) return t.gameObject;
              t = t.parent;
            }
          }
        }
    }
    
    /// <summary>
    /// Collect all objects that are needed to create given roots. 
    /// </summary>
    [PublicAPI]
    public static ImmutableList<Object> collectDependencies(Object[] roots) => 
      EditorUtility.CollectDependencies(roots)
        .Where(o => o is GameObject || o is ScriptableObject)
        .Distinct()
        .ToImmutableList();

    [PublicAPI]
    public static ImmutableList<Error> checkScene(
      Scene scene, Option<CustomObjectValidator> customValidatorOpt = default,
      Action<Progress> onProgress = null, Action onFinish = null
    ) {
      var objects = getSceneObjects(scene);
      var errors = check(
        new CheckContext(scene.name), objects, customValidatorOpt, 
        onProgress, onFinish
      );
      return errors;
    }

    [PublicAPI]
    public static Tpl<ImmutableList<Error>, TimeSpan> checkSceneWithTime(
      Scene scene, Option<CustomObjectValidator> customValidatorOpt = default,
      Action<Progress> onProgress = null, Action onFinish = null
    ) {
      var stopwatch = Stopwatch.StartNew();
      var errors = checkScene(scene, customValidatorOpt, onProgress, onFinish);
      return F.t(errors, stopwatch.Elapsed);
    }

    [PublicAPI]
    public static ImmutableList<Error> checkAssetsAndDependencies(
      IEnumerable<PathStr> assets, Option<CustomObjectValidator> customValidatorOpt = default,
      Action<Progress> onProgress = null, Action onFinish = null
    ) {
      var loadedAssets =
        assets.Select(s => AssetDatabase.LoadMainAssetAtPath(s)).ToArray();
      var dependencies = collectDependencies(loadedAssets);
      return check(
        // and instead of &, because unity does not show '&' in some windows
        new CheckContext("Assets and Deps"),
        dependencies, customValidatorOpt, onProgress, onFinish, UniqueValuesCache.create.some()
      );
    }

    /// <summary>
    /// Check objects and their children.
    /// 
    /// <see cref="check"/>.
    /// </summary>
    [PublicAPI]
    public static ImmutableList<Error> checkRecursively(
      CheckContext context, IEnumerable<Object> objects,
      Option<CustomObjectValidator> customValidatorOpt = default,
      Action<Progress> onProgress = null, Action onFinish = null
    ) => check(
      context,
      collectDependencies(objects.ToArray()),
      customValidatorOpt: customValidatorOpt, onProgress: onProgress, onFinish: onFinish,
      uniqueValuesCache: UniqueValuesCache.create.some()
    );

    /// <summary>
    /// Check given objects. This does not walk through them. <see cref="checkRecursively"/>.
    /// </summary>
    [PublicAPI]
    public static ImmutableList<Error> check(
      CheckContext context, ICollection<Object> objects,
      Option<CustomObjectValidator> customValidatorOpt = default,
      Action<Progress> onProgress = null, Action onFinish = null,
      Option<UniqueValuesCache> uniqueValuesCache = default
    ) {
      Option.ensureValue(ref customValidatorOpt);
      Option.ensureValue(ref uniqueValuesCache);

      var errors = ImmutableList<Error>.Empty;
      var scanned = 0;
      foreach (var o in objects) {
        onProgress?.Invoke(new Progress(scanned++, objects.Count));

        if (o is GameObject go) {
          foreach (var transform in go.transform.andAllChildrenRecursive()) {
            var components = transform.GetComponents<Component>();
            foreach (var c in components) {
              errors =
                c
                ? errors.AddRange(checkComponent(context, c, customValidatorOpt, uniqueValuesCache))
                : errors.Add(Error.missingComponent(transform.gameObject));
            }
          }
        }
        else {
          errors = errors.AddRange(checkComponent(context, o, customValidatorOpt, uniqueValuesCache));
        }
      }

      foreach (var valuesCache in uniqueValuesCache)
        foreach (var df in valuesCache.getDuplicateFields())
          foreach (var obj in df.objectsWithThisValue) {
            errors = errors.Add(Error.duplicateUniqueValueError(df.category, df.fieldValue, obj, context));
          }

      onFinish?.Invoke();

      // FIXME: there should not be a need to a Distinct call here, we have a bug somewhere in the code.
      return errors.Distinct().ToImmutableList();
    }

    /// <summary>
    /// Check one component non-recursively. 
    /// </summary>
    [PublicAPI]
    public static ImmutableList<Error> checkComponent(
      CheckContext context, Object component, Option<CustomObjectValidator> customObjectValidatorOpt, 
      Option<UniqueValuesCache> uniqueCache = default
    ) {
      Option.ensureValue(ref uniqueCache);
      var errors = ImmutableList<Error>.Empty;

      {
        if (component is MonoBehaviour mb) {
          var componentType = component.GetType();
          if (!context.checkedComponentTypes.Contains(item: componentType)) {
            errors = errors.AddRange(items: checkComponentType(context: context, go: mb.gameObject, type: componentType));
            context = context.withCheckedComponentType(c: componentType);
          }
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
          o: component, property: sp.propertyPath, context: context
        ));

      }

      var fieldErrors = validateFields(
        containingComponent: component,
        objectBeingValidated: component,
        createError: new ErrorFactory(component, context),
        customObjectValidatorOpt: customObjectValidatorOpt,
        uniqueValuesCache: uniqueCache
      );
      errors = errors.AddRange(items: fieldErrors);

      return errors;
    }

    public static ImmutableList<Error> checkComponentType(
      CheckContext context, GameObject go, Type type
    ) {
      var requiredComponents = type
        .getAttributes<RequireComponent>(inherit: true)
        .SelectMany(rc => new[] {F.opt(rc.m_Type0), F.opt(rc.m_Type1), F.opt(rc.m_Type2)}.flatten(),
          (rc, requiredType) => new {rc, requiredType})
        .ToArray();
      return requiredComponents
        .Where(@t => !go.GetComponent(@t.requiredType))
        .Select(@t => @t.requiredType)
        .Aggregate(
          ImmutableList<Error>.Empty,
          (current, requiredType) =>
            current.Add(Error.requiredComponentMissing(go, requiredType, type, context))
        );
    }

    static IEnumerable<Error> checkUnityEvent(
      IErrorFactory errorFactory, FieldHierarchyStr fieldHierarchy, UnityEventBase evt
    ) {
      UnityEventReflector.rebuildPersistentCallsIfNeeded(evt);

      var persistentCalls = evt.__persistentCalls();
      var listPersistentCallOpt = persistentCalls.calls;
      foreach (var listPersistentCall in listPersistentCallOpt) {
        var index = 0;
        foreach (var persistentCall in listPersistentCall) {
          if (persistentCall.isValid) {
            if (evt.__findMethod(persistentCall).isNone)
              yield return errorFactory.unityEventInvalidMethod(fieldHierarchy, index);
          }
          else
            yield return errorFactory.unityEventInvalid(fieldHierarchy, index);

          index++;
        }
      }
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
      FieldHierarchy fieldHierarchy = null,
      Option<UniqueValuesCache> uniqueValuesCache = default
    ) {
      Option.ensureValue(ref uniqueValuesCache);
      fieldHierarchy ??= new FieldHierarchy();

      if (objectBeingValidated is OnObjectValidate onObjectValidatable) {
        // Try because custom validations can throw exceptions.
        var validateResult = F.doTry(() => 
          // Force strict enumerable evaluation, because it might throw an exception while evaluating.
          onObjectValidatable.onObjectValidate(containingComponent).ToArray()
        );
        if (validateResult.isSuccess) {
          foreach (var error in validateResult.__unsafeGet) {
            yield return createError.custom(fieldHierarchy.asString(), error, true);
          }
        }
        else {
          var error = validateResult.__unsafeException;
          yield return createError.exceptionInCustomValidator(fieldHierarchy.asString(), error);
        }
      }

      {
        if (objectBeingValidated is UnityEventBase unityEvent) {
          var errors = checkUnityEvent(createError, fieldHierarchy.asString(), unityEvent);
          foreach (var error in errors) yield return error;
        }
      }

      foreach (var customValidator in customObjectValidatorOpt) {
        foreach (var _err in customValidator(containingComponent, objectBeingValidated)) {
          yield return createError.custom(fieldHierarchy.asString(), _err, true);
        }
      }

      var fields = getFilteredFields(objectBeingValidated);
      foreach (var fi in fields) {
        fieldHierarchy.stack.Push(fi.Name);
        var fieldValue = fi.GetValue(objectBeingValidated);
        var hasNonEmpty = fi.hasAttribute<NonEmptyAttribute>();
        
        foreach (var cache in uniqueValuesCache) {
          foreach (var attribute in fi.getAttributes<UniqueValue>()) {
            cache.addCheckedField(attribute.category, fieldValue, containingComponent);
          }
        }
        if (fieldValue is string s) {
          if (fi.getAttributes<UnityTagAttribute>().nonEmptyAllocating()) {
            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(s)) {
              yield return createError.badTextFieldTag(fieldHierarchy.asString());
            }
          }
          
          if (s.isEmpty() && hasNonEmpty)
            yield return createError.emptyString(fieldHierarchy.asString());
        }
        if (fi.isSerializable()) {
          var hasNotNull = fi.hasAttribute<NotNullAttribute>();
          // Sometimes we get empty unity object. Equals catches that
          if (fieldValue == null || fieldValue.Equals(null)) {
            if (hasNotNull) yield return createError.nullField(fieldHierarchy.asString());
          }
          else {
            if (fieldValue is IList list) {
              if (list.Count == 0 && hasNonEmpty) {
                yield return createError.emptyCollection(fieldHierarchy.asString());
              }
              if (listIsSerializable(list)) {
                var fieldValidationResults = validateListElementsFields(
                  containingComponent, list, fi, hasNotNull,
                  fieldHierarchy, createError, customObjectValidatorOpt,
                  uniqueValuesCache
                );
                foreach (var _err in fieldValidationResults) yield return _err;
              }
            }
            else {
              var fieldType = fi.FieldType;
              if (
                typeIsSerializableAsValue(fieldType)
              ) {
                var validationErrors = validateFields(
                  containingComponent, fieldValue, createError,
                  customObjectValidatorOpt, fieldHierarchy,
                  uniqueValuesCache
                );
                foreach (var _err in validationErrors) yield return _err;
              }
            }
            
            static bool listIsSerializable(IList list) {
              var type = list.GetType();
              if (type.IsGenericType) {
                return typeIsSerializableAsValue(type.GenericTypeArguments[0]);
              }
              return false;
            }

            static bool typeIsSerializableAsValue(Type type) =>
              !type.IsPrimitive
              && type.hasAttribute<SerializableAttribute>()
              // sometimes serializable attribute is added on ScriptableObject, we want to skip that
              && !unityObjectType.IsAssignableFrom(type);
          }
        }
        fieldHierarchy.stack.Pop();
      }
    }

    static IEnumerable<FieldInfo> getFilteredFields(object o) {
      if (o == null) return Enumerable.Empty<FieldInfo>();
      var fields = o.GetType().getAllFields();
      foreach (var sv in (o as ISkipObjectValidationFields).opt()) {
        var blacklisted = sv.blacklistedFields();
        return fields.Where(fi => !blacklisted.Contains(fi.Name));
      }
      return fields;
    }

    static readonly Type unityObjectType = typeof(Object);

    static IEnumerable<Error> validateListElementsFields(
      Object containingComponent, IList list, FieldInfo listFieldInfo,
      bool hasNotNull, FieldHierarchy fieldHierarchy,
      IErrorFactory createError,
      Option<CustomObjectValidator> customObjectValidatorOpt,
      Option<UniqueValuesCache> uniqueValuesCache
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
            customObjectValidatorOpt, fieldHierarchy,
            uniqueValuesCache
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
