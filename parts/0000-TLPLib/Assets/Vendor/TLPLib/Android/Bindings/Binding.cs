#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings {
  public abstract class Binding {
    public readonly AndroidJavaObject java;

    protected Binding(AndroidJavaObject java) { this.java = java; }
  }
}
#endif