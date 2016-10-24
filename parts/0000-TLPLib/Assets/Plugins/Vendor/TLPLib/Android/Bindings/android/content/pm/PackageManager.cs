#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content.pm {
  public class PackageManager : Binding {
    public PackageManager(AndroidJavaObject java) : base(java) {}

    public bool hasSystemFeature(string feature) {
      if (Application.isEditor) return true;
      return java.Call<bool>("hasSystemFeature", feature);
    } 
  }
}
#endif