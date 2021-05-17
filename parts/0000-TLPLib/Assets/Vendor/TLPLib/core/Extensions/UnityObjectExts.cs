using System;
using com.tinylabproductions.TLPLib.Filesystem;
using pzd.lib.functional;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityObjectExts {
#if UNITY_EDITOR
    public static PathStr editorAssetPath(this Object obj) => new PathStr(AssetDatabase.GetAssetPath(obj));
#endif

    public static A dontDestroyOnLoad<A>(this A a) where A : Object {
      Object.DontDestroyOnLoad(a);
      return a;
    }

    /** Invoke `f` on `a` if it is not dead. */
    public static B optInvoke<A, B>(this A a, Func<A, B> f)
      where A : Object
      where B : Object
    => a ? f(a) : null;

    public static A assertIsSet<A>(this A obj, string name) where A : Object {
      if (!obj) throw new IllegalStateException($"{name} is not set to an object!");
      return obj;
    }
    
    public static Option<GameObject> getGameObject(this Object obj) =>
      obj switch {
        GameObject go => Some.a(go),
        Component c => Some.a(c.gameObject),
        _ => None._
      };
  }
}
