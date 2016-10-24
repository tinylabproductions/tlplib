#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  public class SharedPreferences : Binding {
    public SharedPreferences(AndroidJavaObject java) : base(java) {}

    public Option<string> getString(string key) =>
      F.opt(java.c<string>("getString", key, null));

    public int getInt(string key, int defaultValue) =>
      java.c<int>("getInt", key, defaultValue);
  }
}

#endif