using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AdvancedInspector;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.validations;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public interface IFieldValidator {
    bool isValid(object value);
  }

  public class NonEmptyValidator : IFieldValidator {
    public bool isValid(object value) {
      {
        if (value is string str && str.isEmpty()) {
          // createError.emptyString(fieldHierarchy.asString());
          return false;
        }
      }
      {
        if (value != null && value is IList list) {
          if (list.Count == 0) {
            return false;
            // yield return createError.emptyCollection(fieldHierarchy.asString());
          }
        }
      }
      return true;
    }
  }

  public class TagValidator : IFieldValidator {
    public bool isValid(object value) {
      if (value is string s) {
        if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(s)) {
          return false;
          //yield return createError.badTextFieldTag(fieldHierarchy.asString());
        }
      }
      return true;
    }
  }

  public class NotNullValidator : IFieldValidator {
    public bool isValid(object value) {
      // Sometimes we get empty unity object. Equals catches that
      if (value == null || value.Equals(null)) {
        return false;
        //yield return createError.nullField(fieldHierarchy.asString());
      }
      return true;
    }
  }

  public class UniqueValuesValidator {
    readonly string category;

    public UniqueValuesValidator(string category) => this.category = category;

    public void add(object value, ObjectValidator.UniqueValuesCache cache, UnityEngine.Object containingComponent) {
      cache.addCheckedField(category, value, containingComponent);
    }
  }

  // WIP
  public partial class FieldValidator {
    [Record]
    public partial struct CacheKey {
      public readonly FieldInfo fi;
      public readonly bool isListElement;
    }

    static readonly Dictionary<CacheKey, FieldValidator> cache = new Dictionary<CacheKey, FieldValidator>();

    public static FieldValidator a(FieldInfo fi, bool isListElement) =>
      cache.getOrUpdate(new CacheKey(fi, isListElement), k => new FieldValidator(k.fi, k.isListElement));

    readonly FieldInfo fi;
    readonly ImmutableArray<IFieldValidator> validators;
    readonly ImmutableArray<UniqueValuesValidator> uniqueValidators;

    FieldValidator(FieldInfo fi, bool isListElement) {
      this.fi = fi;

      var b = ImmutableArray.CreateBuilder<IFieldValidator>();

      if (!isListElement && fi.hasAttribute<NonEmptyAttribute>()) {
        b.Add(new NonEmptyValidator());
      }

      uniqueValidators = fi.getAttributes<UniqueValue>().Select(uv => new UniqueValuesValidator(uv.category))
        .ToImmutableArray();

      if (fi.getAttributes<TextFieldAttribute>().Any(a => a.Type == TextFieldType.Tag)) {
        b.Add(new TagValidator());
      }

      if (fi.hasAttribute<NotNullAttribute>()) {
        b.Add(new NotNullValidator());
      }

      validators = b.ToImmutable();

      /*
      fieldHierarchy.stack.Push(fi.Name);
      var fieldValue = fi.GetValue(objectBeingValidated);
      var hasNonEmpty = fi.hasAttribute<NonEmptyAttribute>();

      foreach (var cache in uniqueValuesCache) {
      }
      if (fieldValue is string s) {
        if (fi.getAttributes<TextFieldAttribute>().Any(a => a.Type == TextFieldType.Tag)) {
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
            var fieldValidationResults = validateListElementsFields(
              containingComponent, list, fi, hasNotNull,
              fieldHierarchy, createError, customObjectValidatorOpt,
              uniqueValuesCache
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
                customObjectValidatorOpt, fieldHierarchy,
                uniqueValuesCache
              );
              foreach (var _err in validationErrors) yield return _err;
            }
          }
        }
        */
    }

    public bool isValid(object value) {
      foreach (var validator in validators) {
        if (!validator.isValid(value)) return false;
      }
      return true;
    }

    public string str() {
      return validators.Select(v => v.GetType().Name).mkString(", ");
    }
  }
}