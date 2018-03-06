#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.app {
  public class Application : Context {
    public Application(AndroidJavaObject java) : base(java) {}
  }
}
#endif