#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using com.tinylabproductions.TLPLib.Android.java.lang;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.app {
  public class Activity : Context {
    public Activity(AndroidJavaObject java) : base(java) {}

    [PublicAPI]
    public Application application => 
      new Application(java.cjo("getApplication"));
    
    [PublicAPI]
    public void runOnUIThread(Action action) =>
      java.Call("runOnUiThread", new Runnable(action));
  }
}
#endif