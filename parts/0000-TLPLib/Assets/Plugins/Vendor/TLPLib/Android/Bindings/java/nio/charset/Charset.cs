#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.nio.charset {
  public class Charset : Binding {
    public Charset(AndroidJavaObject java) : base(java) {}
  }
}
#endif