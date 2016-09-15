#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Logger;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class StackTraceElement : Binding {
    public StackTraceElement(AndroidJavaObject java) : base(java) {}

    public StackTraceElement(
      string declaringClass, string methodName, string fileName, int lineNumber
    ) : this(new AndroidJavaObject(
      "java.lang.StackTraceElement", declaringClass, methodName, fileName, lineNumber
    )) {}
  }

  public static class StackTraceElementExts {
    public static StackTraceElement asAndroid(this BacktraceElem e) => new StackTraceElement(
      e.declaringClass, e.method,
      e.fileInfo.fold("unknown", fi => fi.file),
      e.fileInfo.fold(-1, fi => fi.lineNo)
    );
  }
}
#endif