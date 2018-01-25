using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Data {
  /// <summary>
  /// Mark that A is a prefab.
  /// </summary>
  /// <typeparam name="A"></typeparam>
  public struct TagPrefab<A> : IEquatable<TagPrefab<A>> where A : Object {
    public readonly A prefab;

    public TagPrefab(A prefab) { this.prefab = prefab; }

    #region Equality

    public bool Equals(TagPrefab<A> other) => EqualityComparer<A>.Default.Equals(prefab, other.prefab);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is TagPrefab<A> tagPrefab && Equals(tagPrefab);
    }

    public override int GetHashCode() => EqualityComparer<A>.Default.GetHashCode(prefab);

    public static bool operator ==(TagPrefab<A> left, TagPrefab<A> right) { return left.Equals(right); }
    public static bool operator !=(TagPrefab<A> left, TagPrefab<A> right) { return !left.Equals(right); }

    #endregion

    public override string ToString() => $"{nameof(TagPrefab<A>)}({prefab})";
  }
  public static class TagPrefab {
    public static TagPrefab<A> a<A>(A prefab) where A : Object => new TagPrefab<A>(prefab);
  }
}