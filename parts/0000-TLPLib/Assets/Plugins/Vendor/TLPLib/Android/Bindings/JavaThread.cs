#if UNITY_ANDROID
using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings {
  public class JavaThread : Binding {
    public JavaThread(Action act) 
      : base(new AndroidJavaObject("java.lang.Thread", new JavaRunnable(act)))
    {}

    public void start() => java.Call("start");
    public void stop() => java.Call("stop");
  }
}
#endif