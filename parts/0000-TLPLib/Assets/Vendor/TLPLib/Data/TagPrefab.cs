using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>Mark that A is a prefab.</summary>
  /// You need to extend this class and mark it as <see cref="SerializableAttribute"/>
  /// to serialize it, because Unity does not serialize generic classes.
  [Record]
  public partial class TagPrefab<A> : TagPrefab, OnObjectValidate where A : Object {
    #region Unity Serialized Fields

#pragma warning disable 649
    // ReSharper disable NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
    [SerializeField, NotNull, PublicAccessor] A _prefab;
    // ReSharper restore NotNullMemberIsNotInitialized, FieldCanBeMadeReadOnly.Local, ConvertToConstant.Local
#pragma warning restore 649

    #endregion
    
    [PublicAPI] protected TagPrefab() {}
    
    public IEnumerable<ErrorMsg> onObjectValidate(Object containingComponent) {
#if UNITY_EDITOR
      var type = UnityEditor.PrefabUtility.GetPrefabType(_prefab);
      if (type != UnityEditor.PrefabType.Prefab)
        yield return new ErrorMsg($"Expected {_prefab} to be a prefab, but it was {type}!");
#else
      return System.Linq.Enumerable.Empty<ErrorMsg>();
#endif
    }

    [PublicAPI] public static implicit operator bool(TagPrefab<A> _prefab) => _prefab._prefab;
  }
  
  public abstract class TagPrefab {
    [PublicAPI] public static TagPrefab<A> a<A>(A prefab) where A : Object => new TagPrefab<A>(prefab);
  }

  public static class TagPrefabExts {
    [PublicAPI]
    public static TagPrefab<B> upcastPrefab<A, B>(this TagPrefab<A> aPrefab, B example)
      where A : B 
      where B : Object => 
      TagPrefab.a<B>(aPrefab.prefab);
  }
  
  [Serializable, PublicAPI] public class GameObjectPrefab : TagPrefab<GameObject> { }
  [Serializable, PublicAPI] public class TransformPrefab : TagPrefab<Transform> { }
  [Serializable, PublicAPI] public class RectTransformPrefab : TagPrefab<RectTransform> { }
  [Serializable, PublicAPI] public class ParticleSystemPrefab : TagPrefab<ParticleSystem> { }
}