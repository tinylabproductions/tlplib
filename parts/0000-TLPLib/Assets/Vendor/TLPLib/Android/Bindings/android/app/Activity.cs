#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.Bindings.android.content;
using com.tinylabproductions.TLPLib.Android.java.lang;
using GenerationAttributes;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.app {
  [JavaBinding("android.app.Activity"), PublicAPI]
  public class Activity : Context {
    public Activity(AndroidJavaObject java) : base(java) {}

    public Application application => 
      new Application(java.cjo("getApplication"));
    
    public void runOnUIThread(Action action) =>
      java.Call("runOnUiThread", new Runnable(action));
    
    public Intent getIntent() =>
      new Intent(java.cjo("getIntent"));
  }
}
#endif