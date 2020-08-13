using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using com.tinylabproductions.TLPLib.Extensions;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.validations;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.collection;
using pzd.lib.functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities.Editor {
  public partial class ObjectValidator {
    /// <summary>
    /// Caches structure of our code so that we wouldn't have to constantly use reflection.
    /// </summary>
    public sealed partial class StructureCache {
      // Implementation notes:
      //
      // When adding to concurrent dictionaries we ignore failures because they just mean that some other thread
      // already did the work and our work can be ignored.
      //
      // [LazyProperty] should also be safe, because the worst that can happen is that two threads will calculate
      // the same value because our properties are pure functions.

      public delegate ImmutableArrayC<Field> GetFieldsForType(Type type, StructureCache cache);
      
      readonly GetFieldsForType _getFieldsForType;
      readonly ConcurrentDictionary<System.Type, Type> typeForSystemType = 
        new ConcurrentDictionary<System.Type, Type>();
      readonly ConcurrentDictionary<Type, ImmutableArrayC<Field>> fieldsForType = 
        new ConcurrentDictionary<Type, ImmutableArrayC<Field>>();

      public StructureCache(GetFieldsForType getFieldsForType) => _getFieldsForType = getFieldsForType;

      public Type getTypeFor(System.Type systemType) {
        if (!typeForSystemType.TryGetValue(systemType, out var t)) {
          t = new Type(systemType);
          typeForSystemType.TryAdd(systemType, t);
        }

        return t;
      }

      public ImmutableArrayC<Field> getFieldsFor<A>(A a) => 
        a == null ? ImmutableArrayC<Field>.empty : getFieldsForType(a.GetType());

      public ImmutableArrayC<Field> getFieldsForType(System.Type type) => 
        getFieldsForType(getTypeFor(type));
      
      public ImmutableArrayC<Field> getFieldsForType(Type type) {
        if (!fieldsForType.TryGetValue(type, out var fields)) {
          fields = _getFieldsForType(type, this);
          fieldsForType.TryAdd(type, fields);
        }

        return fields;
      }
      
      public Type getListItemType(IList list) {
        var type = getTypeFor(list.GetType());
        if (type.firstGenericTypeArgument.valueOut(out var genericType)) {
          return getTypeFor(genericType);
        }
        if (type.arrayElementType.valueOut(out var arrayElementType)) {
          return getTypeFor(arrayElementType);
        }
        throw new Exception($"Could not determine IList element type for {type.type.FullName}");
      }

      [Record] public sealed partial class Type {
        public readonly System.Type type;
        
        [LazyProperty] public bool hasSerializableAttribute => type.hasAttribute<SerializableAttribute>();
        [LazyProperty] public bool isUnityObject => unityObjectType.IsAssignableFrom(type);
        
        [LazyProperty] public bool isArray => type.IsArray;
        [LazyProperty] public Option<System.Type> arrayElementType => 
          isArray ? Some.a(type.GetElementType()) : Option<System.Type>.None;
        
        [LazyProperty] public bool isGeneric => type.IsGenericType;
        [LazyProperty] public Option<System.Type> firstGenericTypeArgument => 
          isGeneric ? Some.a(type.GenericTypeArguments[0]) : Option<System.Type>.None;
        
        [LazyProperty] public bool isSerializableAsValue =>
          type.IsPrimitive 
          || type == typeof(string)
          || (
            hasSerializableAttribute
            // sometimes serializable attribute is added on ScriptableObject, we want to skip that
            && !isUnityObject
          );

        static readonly System.Type unityObjectType = typeof(UnityEngine.Object);
      }
      
      [Record(GenerateConstructor = ConstructorFlags.None)] public sealed partial class Field {
        public readonly Type type;
        public readonly FieldInfo fieldInfo;

        public Field(FieldInfo fieldInfo, StructureCache cache) {
          this.fieldInfo = fieldInfo;
          type = cache.getTypeFor(fieldInfo.FieldType);
        }

        [LazyProperty] public bool hasNonEmptyAttribute => fieldInfo.hasAttribute<NonEmptyAttribute>();
        [LazyProperty] public bool hasNotNullAttribute => fieldInfo.hasAttribute<NotNullAttribute>();

        [LazyProperty] public ImmutableArrayC<UniqueValue> uniqueValueAttributes => 
          fieldInfo.getAttributes<UniqueValue>().toImmutableArrayC();

        [LazyProperty] public ImmutableArrayC<UnityTagAttribute> unityTagAttributes => 
          fieldInfo.getAttributes<UnityTagAttribute>().toImmutableArrayC();

        [LazyProperty] public bool isSerializable => fieldInfo.isSerializable();
        [LazyProperty] public bool isSerializableAsReference =>
          isSerializable && fieldInfo.hasAttribute<SerializeReference>();
      }
    }
  }
}