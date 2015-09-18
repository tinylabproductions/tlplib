using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Android {
  /**
   * Better AndroidJavaRunnableProxy which implements standard java.lang.Object 
   * methods in case someone wants to call them.
   **/
#if UNITY_ANDROID
  public class JavaRunnable : AndroidJavaProxy {
    public readonly Action runnable;

    public JavaRunnable(Action runnable) : base("java/lang/Runnable") {
      this.runnable = runnable;
    }

    public static JavaRunnable a(Action runnable) { return new JavaRunnable(runnable); }

    /* May be called from Java side. */
    public string toString() { return ToString(); }
    public int hashCode() { return GetHashCode(); }
    public bool equals(object o) { return this == o; }

    /* Called from Java side. */
    public void run() { runnable(); }
  }

  public static class JavaRunnableExts {
    public static JavaRunnable toJavaRunnable(this Action runnable) {
      return new JavaRunnable(runnable);
    }
  }
#endif
}
