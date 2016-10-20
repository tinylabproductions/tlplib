#if UNITY_ANDROID
using System.Linq;
using com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.crash_reporting;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.firebase.crash {
  public static class FirebaseCrash {
    static readonly AndroidJavaClass klass = 
      new AndroidJavaClass("com.google.firebase.crash.FirebaseCrash");

    public static void report(Throwable throwable) {
      klass.CallStatic("report", throwable.java);
      if (Log.isDebug) Log.rdebug(
        $"[{nameof(FirebaseCrash)}] reported: {throwable}\n" +
        $"Android Stacktrace: \n" +
        $"  {throwable.getStackTrace().Select(ste => ste.ToString()).mkString("\n  ")}"
      );
    }

    public static bool isSingletonInitialized =>
      klass.CallStatic<bool>("isSingletonInitialized");

    public static ErrorReporter.OnError createOnError() =>
      data => report(UnityError.fromErrorData(data));
  }
}
#endif
      