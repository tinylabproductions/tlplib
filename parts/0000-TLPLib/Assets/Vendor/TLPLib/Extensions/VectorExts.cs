using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class VectorExts {
    public static Vector2 with2(
      this Vector2 v,
#if UNITY_IOS
      Option<float> x = null,
      Option<float> y = null
#else
      Option<float> x = new Option<float>(), 
      Option<float> y = new Option<float>()
#endif
    ) {
#if UNITY_IOS
      if (null == x) x = new Option<float>();
      if (null == y) y = new Option<float>();
#endif
      return new Vector3(x.getOrElse(v.x), y.getOrElse(v.y));
    }

    public static Vector3 with3(
      this Vector3 v,
#if UNITY_IOS
      Option<float> x = null,
      Option<float> y = null,
      Option<float> z = null
#else
      Option<float> x = new Option<float>(), 
      Option<float> y = new Option<float>(),
      Option<float> z = new Option<float>()
#endif
    ) {
#if UNITY_IOS
      if (null == x) x = new Option<float>();
      if (null == y) y = new Option<float>();
      if (null == z) z = new Option<float>();
#endif
      return new Vector3(x.getOrElse(v.x), y.getOrElse(v.y), z.getOrElse(v.z));
    }
  }
}
