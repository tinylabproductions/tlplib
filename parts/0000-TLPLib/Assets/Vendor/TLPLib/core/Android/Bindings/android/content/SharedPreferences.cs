#if UNITY_ANDROID
using pzd.lib.functional;
using com.tinylabproductions.TLPLib.Functional;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.content {
  public class SharedPreferences : Binding {
    public SharedPreferences(AndroidJavaObject java) : base(java) {}

    [PublicAPI]
    public Option<string> getString(string key) =>
      F.opt(java.c<string>("getString", key, null));

    [PublicAPI]
    public int getInt(string key, int defaultValue) =>
      java.c<int>("getInt", key, defaultValue);
    
    [PublicAPI]
    public bool getBool(string key, bool defaultValue) => 
      java.c<bool>("getBoolean", key, defaultValue);
    
    [PublicAPI]
    public Editor edit() => new Editor(java.cjo("edit"));
      
    public class Editor : Binding {
      public Editor(AndroidJavaObject java) : base(java) {}
      
      [PublicAPI]
      public bool commit() => java.c<bool>("commit");
      
      [PublicAPI]
      public Editor putString(string key, string value) {
        java.cjo("putString", key, value);
        return this;
      }
      
      [PublicAPI]
      public Editor putBool(string key, bool value) {
        java.cjo("putBoolean", key, value);
        return this;
      }
      
      [PublicAPI]
      public Editor putInt(string key, int value) {
        java.cjo("putInt", key, value);
        return this;
      }

      [PublicAPI]
      public Editor remove(string key) {
        java.cjo("remove", key);
        return this;
      }
    }
  }
}
#endif