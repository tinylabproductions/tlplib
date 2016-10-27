#if UNITY_ANDROID
using System;
using com.tinylabproductions.TLPLib.Android.Bindings.java.lang;
using UnityEngine;
using ULog = com.tinylabproductions.TLPLib.Logger.Log;

namespace com.tinylabproductions.TLPLib.Android.Bindings.android.util {
  public static class Log {
    // ReSharper disable once EnumUnderlyingTypeIsInt - just to make sure we don't forget.
    public enum Level : int {
      VERBOSE = 2,
      DEBUG = 3,
      INFO = 4,
      WARN = 5,
      ERROR = 6,
      ASSERT = 7
    }

    static readonly AndroidJavaClass klass = new AndroidJavaClass("android.util.Log");

    public static Level fromLoggerLevel(ULog.Level level) {
      switch (level) {
        case ULog.Level.NONE: return Level.ASSERT;
        case ULog.Level.ERROR: return Level.ERROR;
        case ULog.Level.WARN: return Level.WARN;
        case ULog.Level.INFO: return Level.INFO;
        case ULog.Level.DEBUG: return Level.DEBUG;
        case ULog.Level.VERBOSE: return Level.VERBOSE;
        default:
          throw new ArgumentOutOfRangeException(nameof(level), level, null);
      }
    }

    public static int v(string tag, string msg) => klass.CallStatic<int>("v", tag, msg);
    public static int v(string tag, string msg, Throwable t) => klass.CallStatic<int>("v", tag, msg, t.java);
    public static int d(string tag, string msg) => klass.CallStatic<int>("d", tag, msg);
    public static int d(string tag, string msg, Throwable t) => klass.CallStatic<int>("d", tag, msg, t.java);
    public static int i(string tag, string msg) => klass.CallStatic<int>("i", tag, msg);
    public static int i(string tag, string msg, Throwable t) => klass.CallStatic<int>("i", tag, msg, t.java);
    public static int w(string tag, string msg) => klass.CallStatic<int>("w", tag, msg);
    public static int w(string tag, Throwable t) => klass.CallStatic<int>("w", tag, t.java);
    public static int w(string tag, string msg, Throwable t) => klass.CallStatic<int>("w", tag, msg, t.java);
    public static int e(string tag, string msg) => klass.CallStatic<int>("e", tag, msg);
    public static int e(string tag, string msg, Throwable t) => klass.CallStatic<int>("e", tag, msg, t.java);
    public static int wtf(string tag, string msg) => klass.CallStatic<int>("wtf", tag, msg);
    public static int wtf(string tag, Throwable t) => klass.CallStatic<int>("wtf", tag, t.java);
    public static int wtf(string tag, string msg, Throwable t) => klass.CallStatic<int>("wtf", tag, msg, t.java);

    public static string getStackTraceString(Throwable tr) =>
      klass.CallStatic<string>("getStackTraceString", tr.java);

    public static bool isLoggable(string tag, Level level) =>
      klass.CallStatic<bool>("isLoggable", tag, (int) level);

    public static void println(Level priority, string tag, string msg) =>
      klass.CallStatic("println", (int) priority, tag, msg);
  }
}
#endif