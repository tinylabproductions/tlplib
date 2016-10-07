#if UNITY_ANDROID
using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings {
  public class Activity : Context {
    public Activity(AndroidJavaObject java) : base(java) {}

    public void runOnUIThread(Action action) => 
      java.Call("runOnUiThread", new JavaRunnable(action));
  }
}
#endif