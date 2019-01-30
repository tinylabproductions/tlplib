using UnityEngine;

#if UNITY_ANDROID
namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  public class Intent : Binding {
    public Intent(AndroidJavaObject java) : base(java) { }
  }
}
#endif