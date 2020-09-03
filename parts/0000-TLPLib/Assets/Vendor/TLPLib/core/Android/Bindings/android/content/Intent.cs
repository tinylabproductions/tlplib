#if UNITY_ANDROID
using pzd.lib.functional;
using com.tinylabproductions.TLPLib.Android.Bindings.android.net;
using pzd.lib.exts;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  // https://developer.android.com/reference/android/content/Intent.html
  [JavaBinding("android.content.Intent"), PublicAPI]
  public class Intent : Binding {
    public Intent(AndroidJavaObject java) : base(java) { }

    public Option<string> getAction() => java.Call<string>("getAction").opt();
    
    public Option<Uri> getData() {
      var jUri = java.cjo("getData");
      return jUri == null ? F.none_ : F.some(new Uri(jUri));
    }

    public Option<string> getDataString() => java.Call<string>("getDataString").opt();
  }
}
#endif