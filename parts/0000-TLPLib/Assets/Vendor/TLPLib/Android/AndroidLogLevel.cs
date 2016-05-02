using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Android {
  /** Log levels as defined in android.util.Log **/
  public enum AndroidLogLevel {
    ASSERT = 7, DEBUG = 3, ERROR = 6, INFO = 4, VERBOSE = 2, WARN = 5
  }

  public static class AndroidLogLevels {
    public static readonly IEnumerable<AndroidLogLevel> levels = new[] {
      AndroidLogLevel.ASSERT, AndroidLogLevel.ERROR, AndroidLogLevel.WARN,
      AndroidLogLevel.INFO, AndroidLogLevel.DEBUG, AndroidLogLevel.VERBOSE
    };
  }
}
