#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.crash_reporting;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.firebase.crash {
  public static class FirebaseCrash {
    static readonly AndroidJavaClass klass = 
      new AndroidJavaClass("com.google.firebase.crash.FirebaseCrash");

    public static void report(Throwable throwable) {
      klass.CallStatic("report", throwable.java);
      if (Log.isDebug) Log.rdebug($"Reported to firebase: {throwable}");
    }

    public static bool isSingletonInitialized =>
      klass.CallStatic<bool>("isSingletonInitialized");

    public static ErrorReporter.OnError createOnError() =>
      data => report(new UnityError(data));
  }
}
#endif