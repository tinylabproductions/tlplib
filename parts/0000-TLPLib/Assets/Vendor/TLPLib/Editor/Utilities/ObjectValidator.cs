#define DO_PROFILE
﻿using System.Linq;
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
using pzd.lib.collection;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.utils;
using Smooth.Collections;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public static partial class ObjectValidator {
    public static readonly Action<Progress> DEFAULT_ON_PROGRESS = progress => {
      // calling DisplayProgressBar too often affects performance
      if (progress.currentIdx % 100 == 0) {
        EditorUtility.DisplayProgressBar(
          "Validating Objects", $"{progress.currentIdx} / {progress.total}", progress.ratio
        );
      }
    };
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
          var maybeGo = getGameObject(error.obj);
          var context = maybeGo.valueOut(out var go) && go.scene.name == null 
            ? getRootGO(go) // get root GameObject on prefabs
            : error.obj;
          log.log(level, LogEntry.simple(error.ToString(), context: context));
          
          static GameObject getRootGO(GameObject go) {
            var t = go.transform;
            while (true) {
              if (t.parent == null) return t.gameObject;
              t = t.parent;
            }
          }

          static Option<GameObject> getGameObject(Object obj) {
            if (!obj) return F.none_;
            return obj switch {
              GameObject go => go.some(),
              Component c => F.opt(c.gameObject),
              _ => F.none_
            };
          }
        }
    }
    
    /// <summary>
    /// Collect all objects that are needed to create given roots. 
    /// </summary>
    [PublicAPI]
    public static ImmutableArray<Object> collectDependencies(Object[] roots) => 
      EditorUtility.CollectDependencies(roots)
        .Where(o => o is GameObject || o is ScriptableObject)
        .Distinct()
        .ToImmutableArray();

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

      var errors = new List<Error>();
      var structureCache = new StructureCache(
        getFieldsForType: type => 
          type.type.getAllFields().Select(fi => new StructureCache.Field(type, fi)).toImmutableArrayC()
      );
      var unityTags = UnityEditorInternal.InternalEditorUtility.tags.ToImmutableHashSet();
      var scanned = 0;
      foreach (var o in objects) {
        onProgress?.Invoke(new Progress(scanned++, objects.Count));

        if (o is GameObject go) {
          foreach (var transform in go.transform.andAllChildrenRecursive()) {
            var components = transform.GetComponents<Component>();
            foreach (var c in components) {
              if (c) {
                checkComponent(context, c, customValidatorOpt, errors, structureCache, unityTags, uniqueValuesCache);
              }
              else {
                errors.Add(Error.missingComponent(transform.gameObject));
              }
            }
          }
        }
        else {
          checkComponent(context, o, customValidatorOpt, errors, structureCache, unityTags, uniqueValuesCache);
        }
      }

      foreach (var valuesCache in uniqueValuesCache)
        foreach (var df in valuesCache.getDuplicateFields())
          foreach (var obj in df.objectsWithThisValue) {
            errors.Add(Error.duplicateUniqueValueError(df.category, df.fieldValue, obj, context));
          }

      onFinish?.Invoke();

      // FIXME: there should not be a need to a Distinct call here, we have a bug somewhere in the code.
      return errors.Distinct().ToImmutableList();
    }

    /// <summary>
    /// Check one component non-recursively. 
    /// </summary>
    [PublicAPI]
    public static void checkComponent(
      CheckContext context, Object component, Option<CustomObjectValidator> customObjectValidatorOpt, 
      List<Error> errors, StructureCache structureCache, ImmutableHashSet<string> unityTags, 
      Option<UniqueValuesCache> uniqueCache = default
    ) {
      Option.ensureValue(ref uniqueCache);
      {
        if (component is MonoBehaviour mb)
#if DO_PROFILE
          using (new ProfiledScope(nameof(checkRequireComponents)))
#endif
          {
            var componentType = structureCache.getTypeFor(component.GetType());
            checkRequireComponents(context: context, go: mb.gameObject, type: componentType.type, errors);
            // checkRequireComponents should be called every time
            // if (!context.checkedComponentTypes.Contains(item: componentType)) {
            //   errors = errors.AddRange(items: checkComponentType(context: context, go: mb.gameObject, type: componentType));
            //   context = context.withCheckedComponentType(c: componentType);
            // }
          }
      }

#if DO_PROFILE
      using (new ProfiledScope("Serialized object"))
#endif
      {
        SerializedObject serObj;
        
#if DO_PROFILE
        using (new ProfiledScope("Create serialized object"))
#endif
        {
          serObj = new SerializedObject(obj: component);
        }

        SerializedProperty sp;
        
#if DO_PROFILE
        using (new ProfiledScope("Get iterator"))
#endif
        {
          sp = serObj.GetIterator();
        }

        var isPlayableDirector = component is PlayableDirector;

#if DO_PROFILE
        using (new ProfiledScope("Iteration"))
#endif
        {
          while (sp.NextVisible(enterChildren: true)) {
            if (isPlayableDirector && sp.name == "m_SceneBindings") {
              // skip Scene Bindings of PlayableDirector, because they often have missing references
              if (!sp.NextVisible(enterChildren: false)) break;
            }

            if (
              sp.propertyType == SerializedPropertyType.ObjectReference
              && !sp.objectReferenceValue
              && sp.objectReferenceInstanceIDValue != 0
            ) {
              errors.Add(Error.missingReference(o: component, property: sp.propertyPath, context: context));
            }
          }
        }
      }

      validateFields(
        containingComponent: component,
        objectBeingValidated: component,
        createError: new ErrorFactory(component, context),
        errors, structureCache, unityTags,
        customObjectValidatorOpt: customObjectValidatorOpt,
        uniqueValuesCache: uniqueCache
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

    public readonly struct FieldHierarchyStr {
      public readonly string s;
      public FieldHierarchyStr(string s) { this.s = s; }
      public override string ToString() => $"{nameof(FieldHierarchy)}({s})";
    }

    class FieldHierarchy {
      public readonly Stack<string> stack = new Stack<string>();

      public FieldHierarchyStr asString() => new FieldHierarchyStr(stack.Reverse().mkString('.'));
    }

    static void validateFields(
      Object containingComponent,
      object objectBeingValidated,
      IErrorFactory createError,
      List<Error> errors,
      StructureCache structureCache,
      ImmutableHashSet<string> unityTags,
      Option<CustomObjectValidator> customObjectValidatorOpt,
      FieldHierarchy fieldHierarchy = null,
      Option<UniqueValuesCache> uniqueValuesCache = default
    ) {
#if DO_PROFILE
      using var _ = new ProfiledScope(nameof(validateFields));
#endif
      Option.ensureValue(ref uniqueValuesCache);
      fieldHierarchy ??= new FieldHierarchy();

      if (objectBeingValidated is OnObjectValidate onObjectValidatable) {
#if DO_PROFILE
        using (new ProfiledScope(nameof(OnObjectValidate)))
#endif
        {
          // Try because custom validations can throw exceptions.
          try {
            var objectErrors = onObjectValidatable.onObjectValidate(containingComponent);
            errors.AddRange(objectErrors.Select(error => createError.custom(fieldHierarchy.asString(), error, true)));
          }
          catch (Exception error) {
            errors.Add(createError.exceptionInCustomValidator(fieldHierarchy.asString(), error));
          }
        }
      }

      {
        if (objectBeingValidated is UnityEventBase unityEvent) {
#if DO_PROFILE
          using (new ProfiledScope(nameof(checkUnityEvent)))
#endif
          {
            errors.AddRange(checkUnityEvent(createError, fieldHierarchy.asString(), unityEvent));
          }
        }
      }

      foreach (var customValidator in customObjectValidatorOpt) {
#if DO_PROFILE
        using (new ProfiledScope(nameof(customValidator)))
#endif
        {
          errors.AddRange(
            customValidator(containingComponent, objectBeingValidated)
              .Select(_err => createError.custom(fieldHierarchy.asString(), _err, true))
          );
        }
      }

      ImmutableArrayC<StructureCache.Field> fields;
#if DO_PROFILE
      using (new ProfiledScope("get object fields"))
#endif
      {
        fields = structureCache.getFieldsFor(objectBeingValidated);
      }

      HashSet<string> blacklistedFields;
#if DO_PROFILE
      using (new ProfiledScope("get blacklisted object fields"))
#endif
      {
        blacklistedFields = new HashSet<string>();
        if (objectBeingValidated is ISkipObjectValidationFields svf) 
          blacklistedFields.AddAll(svf.blacklistedFields());
      }
      
      foreach (var field in fields) {
        validateField(
          containingComponent, objectBeingValidated, createError, errors, structureCache, unityTags,
          customObjectValidatorOpt, fieldHierarchy, uniqueValuesCache, blacklistedFields, field
        );
      }
    }

    static void validateField(
      Object containingComponent, object objectBeingValidated, IErrorFactory createError,
      List<Error> errors, StructureCache structureCache, ImmutableHashSet<string> unityTags, 
      Option<CustomObjectValidator> customObjectValidatorOpt,
      FieldHierarchy fieldHierarchy, Option<UniqueValuesCache> uniqueValuesCache, HashSet<string> blacklistedFields, 
      StructureCache.Field field
    ) {
#if DO_PROFILE
      using var _ = new ProfiledScope(nameof(validateField));
#endif
      if (blacklistedFields.Contains(field.fieldInfo.Name)) return;

      fieldHierarchy.stack.Push(field.fieldInfo.Name);
      var fieldValue = field.fieldInfo.GetValue(objectBeingValidated);

      {
        if (uniqueValuesCache.valueOut(out var cache)) {
#if DO_PROFILE
          using (new ProfiledScope(nameof(uniqueValuesCache)))
#endif
          {
            foreach (var attribute in field.uniqueValueAttributes) {
              cache.addCheckedField(attribute.category, fieldValue, containingComponent);
            }
          }
        }
      }
      if (fieldValue is string s) {
#if DO_PROFILE
        using (new ProfiledScope(nameof(field.unityTagAttributes)))
#endif
        {
          var unityTagAttributes = field.unityTagAttributes;
          if (unityTagAttributes.nonEmpty()) {
            if (!unityTags.Contains(s)) {
              errors.Add(createError.badTextFieldTag(fieldHierarchy.asString()));
            }
          }
        }

        if (field.hasNonEmptyAttribute && s.isEmpty()) {
          errors.Add(createError.emptyString(fieldHierarchy.asString()));
        }
      }

      if (field.isSerializable) {
#if DO_PROFILE
        using (new ProfiledScope(nameof(field.isSerializable)))
#endif
        {
          switch (fieldValue) {
            case null:
            // Sometimes we get empty unity object.
            case Object unityObj when !unityObj: {
              if (field.hasNotNullAttribute) errors.Add(createError.nullField(fieldHierarchy.asString()));
              break;
            }
            case IList list: {
              if (list.Count == 0) {
                if (field.hasNonEmptyAttribute) {
                  errors.Add(createError.emptyCollection(fieldHierarchy.asString()));
                }
              }
              else {
                validateListElementsFields(
                  containingComponent, list, field.hasNotNullAttribute,
                  fieldHierarchy, createError, errors, structureCache, unityTags,
                  customObjectValidatorOpt, uniqueValuesCache
                );
              }

              break;
            }
            default: {
              if (field.type.isSerializableAsValue) {
                validateFields(
                  containingComponent, fieldValue, createError, errors, structureCache, unityTags,
                  customObjectValidatorOpt, fieldHierarchy, uniqueValuesCache
                );
              }

              break;
            }
          }
        }
      }

      fieldHierarchy.stack.Pop();
    }

    static void validateListElementsFields(
      Object containingComponent, IList list,
      bool hasNotNull, FieldHierarchy fieldHierarchy,
      IErrorFactory createError, List<Error> errors, StructureCache structureCache, ImmutableHashSet<string> unityTags,
      Option<CustomObjectValidator> customObjectValidatorOpt,
      Option<UniqueValuesCache> uniqueValuesCache
    ) {
#if DO_PROFILE
      using var _ = new ProfiledScope(nameof(validateListElementsFields));
#endif
      var listItemType = structureCache.getListItemType(list);

      if (listItemType.isUnityObject) {
        if (hasNotNull && list.Contains(null))
          errors.Add(createError.nullField(fieldHierarchy.asString()));
      }
      else if (listItemType.isSerializableAsValue) {
        var index = 0;
        foreach (var listItem in list) {
          fieldHierarchy.stack.Push($"[{index}]");
          validateFields(
            containingComponent, listItem, createError, errors, structureCache, unityTags,
            customObjectValidatorOpt, fieldHierarchy,
            uniqueValuesCache
          );
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
