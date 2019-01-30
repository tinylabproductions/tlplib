#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using GenerationAttributes;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.app {

  [JavaBinding("android.app.Application")]
  public class Application : Context {
    public Application(AndroidJavaObject java) : base(java) {}
  }
}
#endif