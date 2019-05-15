#if UNITY_ANDROID
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.net {
  // https://developer.android.com/reference/android/net/Uri.html
  [JavaBinding("android.net.Uri"), PublicAPI]
  public class Uri : Binding {
    static readonly AndroidJavaClass klass = new AndroidJavaClass("android.net.Uri");

    public Uri(AndroidJavaObject java) : base(java) {}

    public static Uri parse(string uriString) => new Uri(klass.csjo("parse", uriString));
  }
}
#endif