using com.tinylabproductions.TLPLib.Filesystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityObjectExts {
#if UNITY_EDITOR
    public static PathStr path(this Object obj) {
      return new PathStr(AssetDatabase.GetAssetPath(obj));
    }
#endif

    public static A dontDestroyOnLoad<A>(this A a) where A : Object {
      Object.DontDestroyOnLoad(a);
      return a;
    }
  }
}
