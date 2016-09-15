#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class Throwable : Binding {
    public Throwable(AndroidJavaObject java) : base(java) {}
    public Throwable(string message) : this(new AndroidJavaObject("java.lang.Throwable", message)) {}
  }
}
#endif