using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace com.tinylabproductions.TLPGame {
  public abstract class ResourceReference<A> : ScriptableObject where A : Object {
#pragma warning disable 649
    [SerializeField, NotNull] A _reference;
#pragma warning restore 649

    public A reference => _reference;

#if UNITY_EDITOR
    public A editorReference {
      get { return _reference; }
      set { _reference = value; }
    }
#endif
  }

  public static class ResourceReference {
#if UNITY_EDITOR
    public static SO create<SO, A>(string path, A reference) 
      where SO : ResourceReference<A> where A : Object
    {
      var so = ScriptableObject.CreateInstance<SO>();
      so.editorReference = reference;
      AssetDatabase.CreateAsset(so, path);
      return so;
    }
#endif
  }
}