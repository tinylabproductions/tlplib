#if UNITY_ANDROID
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.crash_reporting;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.google.firebase.crash {
  public interface IFirebaseCrash {
    void report(Throwable throwable);
    bool isSingletonInitialized { get; }
    ErrorReporter.OnError createOnError();
  }

  public class FirebaseCrashNoOp : IFirebaseCrash {
    public void report(Throwable throwable) { }
    public bool isSingletonInitialized => false;
    public ErrorReporter.OnError createOnError() => _ => { };
  }

  public class FirebaseCrash : IFirebaseCrash {
    static readonly AndroidJavaClass klass = 
      new AndroidJavaClass("com.google.firebase.crash.FirebaseCrash");

    public void report(Throwable throwable) {
      klass.CallStatic("report", throwable.java);
      if (Log.isDebug) Log.rdebug(
        $"[{nameof(FirebaseCrash)}] reported: {throwable}\n" +
        $"Android Stacktrace: \n" +
        $"  {throwable.getStackTrace().Select(ste => ste.ToString()).mkString("\n  ")}"
      );
    }

    public bool isSingletonInitialized =>
      klass.CallStatic<bool>("isSingletonInitialized");

    public ErrorReporter.OnError createOnError() =>
      data => report(UnityError.fromErrorData(data));
  }
}
#endif
