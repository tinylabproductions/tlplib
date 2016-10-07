#if UNITY_ANDROID
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using com.tinylabproductions.TLPLib.Android.Bindings.java.util;
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.com.tinylabproductions.tlplib.crash_reporting {
  public class UnityError : Throwable {
    public static UnityError fromErrorData(ErrorReporter.ErrorData data) {
      // We need different classes because otherwise Firebase groups all different log 
      // types together by stacktrace.
      switch (data.errorType) {
        case LogType.Assert: return new UnityError("UnityAssert", data);
        case LogType.Error: return new UnityError("UnityError", data);
        case LogType.Exception: return new UnityError("UnityException", data);
        case LogType.Log: return new UnityError("UnityLog", data);
        case LogType.Warning: return new UnityError("UnityWarning", data);
      }
      return new UnityError("UnityUnknown", data);
    }

    public readonly ErrorReporter.ErrorData data;
    public readonly string className;

    UnityError(string className, ErrorReporter.ErrorData data) : base(new AndroidJavaObject(
      $"com.tinylabproductions.tlplib.crash_reporting.{className}",
      data.message
    )) {
      this.className = className;
      this.data = data;

      var stacktrace = data.backtrace.Select(elem => elem.asAndroid()).ToImmutableList();
      setStackTrace(stacktrace);
    }

    public void setStackTrace(ICollection<StackTraceElement> stackTrace) {
      // Can't call setStackTrace directly, because 
      // http://forum.unity3d.com/threads/passing-arrays-through-the-jni.91757/#post-1899528
      var arrayList = new ArrayList(stackTrace.Select(e => e.java), stackTrace.Count);
      java.Call("setStackTraceElems", arrayList.java);
    }

    public override string ToString() => $"{nameof(UnityError)}[{data}]";
  }
}
#endif