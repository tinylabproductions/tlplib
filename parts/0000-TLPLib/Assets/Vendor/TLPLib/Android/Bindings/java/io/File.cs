#if UNITY_ANDROID
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.io {
  public class File : Binding {
    public File(AndroidJavaObject java) : base(java) {}

    public string getPath() => java.Call<string>("getPath");
  }
}
#endif