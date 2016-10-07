#if UNITY_ANDROID
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.util {
  public class HashMap : Binding {
    public HashMap(AndroidJavaObject java) : base(java) {}
    public HashMap() : base(new AndroidJavaObject("java.util.HashMap")) {}

    public HashMap(
      IEnumerable<KeyValuePair<AndroidJavaObject, AndroidJavaObject>> enumerable
    ) : this() {
      foreach (var kv in enumerable) put(kv.Key, kv.Value);
    }

    public Option<AndroidJavaObject> put(AndroidJavaObject key, AndroidJavaObject value) =>
      F.opt(java.cjoReturningNull("put", key, value));
  }
}
#endif
      