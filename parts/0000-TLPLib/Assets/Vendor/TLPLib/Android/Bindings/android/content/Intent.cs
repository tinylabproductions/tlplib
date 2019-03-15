#if UNITY_ANDROID
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  [JavaBinding("android.content.Intent"), PublicAPI]
  public class Intent : Binding {
    public Intent(AndroidJavaObject java) : base(java) { }

    public string getAction() => java.Call<string>("getAction");
  }
}
#endif