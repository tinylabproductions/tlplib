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
    public readonly ErrorReporter.ErrorData data;

    public UnityError(ErrorReporter.ErrorData data) : base(new AndroidJavaObject(
      "com.tinylabproductions.tlplib.crash_reporting.UnityError",
      data.errorType.ToString(), data.message
    )) {
      this.data = data;
      setStackTrace(data.backtrace.Select(elem => elem.asAndroid()).ToImmutableList());
    }

    public void setStackTrace(ICollection<StackTraceElement> stackTrace) {
      // Can't call setStackTrace directly, because 
      // http://forum.unity3d.com/threads/passing-arrays-through-the-jni.91757/#post-1899528
      var arrayList = new ArrayList(stackTrace.Count);
      foreach (var elem in stackTrace) arrayList.add(elem.java);
      java.Call("setStackTraceElems", arrayList.java);
    }

    public override string ToString() => 
      $"{nameof(UnityError)}[{data}]";
  }
}
#endif