#if UNITY_ANDROID
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.net {
  // https://developer.android.com/reference/android/net/Uri.html
  [JavaBinding("android.net.Uri"), PublicAPI]
  public class Uri : Binding {
    public Uri(AndroidJavaObject java) : base(java) {}
  }
}
#endif