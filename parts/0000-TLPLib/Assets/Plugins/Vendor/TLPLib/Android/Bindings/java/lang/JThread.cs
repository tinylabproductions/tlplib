#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.java.lang;
using com.tinylabproductions.TLPLib.Data;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class JThread : Binding {
    const string JAVA_CLASS_NAME = "java.lang.Thread";
    static readonly AndroidJavaClass klass = new AndroidJavaClass(JAVA_CLASS_NAME);

    public JThread(Action act) 
      : base(new AndroidJavaObject(JAVA_CLASS_NAME, new Runnable(act)))
    {}

    public void start() => java.Call("start");
    public void stop() => java.Call("stop");

    public static void sleep(long millis) => klass.CallStatic("sleep", millis);
    public static void sleep(Duration duration) => sleep(duration.millis);
  }
}
#endif