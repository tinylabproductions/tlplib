#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.android.net;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  // https://developer.android.com/reference/android/content/Intent.html
  [JavaBinding("android.content.Intent"), PublicAPI]
  public class Intent : Binding {
    public Intent(AndroidJavaObject java) : base(java) { }

    public string getAction() => java.Call<string>("getAction");
    public Uri getData() => new Uri(java.cjo("getData"));
    public string getDataString() => java.Call<string>("getDataString");
  }
}
#endif