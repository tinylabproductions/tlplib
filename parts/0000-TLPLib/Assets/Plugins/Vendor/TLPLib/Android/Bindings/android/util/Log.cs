#if UNITY_ANDROID
using System;
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
  }
}
#endif