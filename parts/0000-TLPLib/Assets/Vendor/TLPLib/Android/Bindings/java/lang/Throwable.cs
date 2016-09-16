#if UNITY_ANDROID
using com.tinylabproductions.TLPLib.Android.Bindings.java.io;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android.Bindings.java.lang {
  public class Throwable : Binding {
    public Throwable(AndroidJavaObject java) : base(java) {}
    public Throwable(string message) : this(new AndroidJavaObject("java.lang.Throwable", message)) {}

    public string stacktraceString { get {
      var sw = new StringWriter();
      printStackTrace(new PrintWriter(sw));
      return sw.ToString();
    } }

    public void printStackTrace(PrintWriter s) => java.Call("printStackTrace", s.java);

    public StackTraceElement[] getStackTrace() =>
      java.Call<AndroidJavaObject[]>("getStackTrace")
      .map(ajo => new StackTraceElement(ajo));
  }
}
#endif