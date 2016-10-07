#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.nio.charset {
  public static class StandardCharsets {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("java.nio.charset.StandardCharsets");

    public static Charset UTF_8 = new Charset(klass.GetStatic<AndroidJavaObject>("UTF_8"));
  }
}
#endif