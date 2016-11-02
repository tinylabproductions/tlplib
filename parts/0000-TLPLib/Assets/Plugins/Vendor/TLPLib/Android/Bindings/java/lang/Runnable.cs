#if UNITY_ANDROID
using System;

namespace com.tinylabproductions.TLPLib.Android.java.lang {
  /**
   * Better AndroidJavaRunnableProxy which implements standard java.lang.Object 
   * methods in case someone wants to call them.
   **/
  public class Runnable : JavaProxy {
    public readonly Action runnable;

    public Runnable(Action runnable) : base("java/lang/Runnable") {
      this.runnable = runnable;
    }

    public static Runnable a(Action runnable) => new Runnable(runnable);

    /* Called from Java side. */
    public void run() => runnable();
  }

  public static class JavaRunnableExts {
    public static Runnable toJavaRunnable(this Action runnable) => new Runnable(runnable);
  }
}
#endif