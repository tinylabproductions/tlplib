using UnityEngine;

namespace com.tinylabproductions.TLPLib.Data {
  /** A is a prefab. */
  public struct TagPrefab<A> where A : Object {
    public readonly A prefab;

    public TagPrefab(A prefab) { this.prefab = prefab; }

    public override string ToString() => $"{nameof(TagPrefab<A>)}({prefab})";

    public TagInstance<A> instantiate() => new TagInstance<A>(Object.Instantiate(prefab));
  }
  public static class TagPrefab {
    public static TagPrefab<A> a<A>(A prefab) where A : Object => new TagPrefab<A>(prefab);
  }

  public struct TagInstance<A> where A : Object {
    public readonly A instance;

    public TagInstance(A instance) { this.instance = instance; }

    public override string ToString() => $"{nameof(TagInstance<A>)}({instance})";
  }
  public static class TagInstance {
    public static TagInstance<A> a<A>(A instance) where A : Object => new TagInstance<A>(instance);
  }
}