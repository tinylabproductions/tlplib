using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;

namespace com.tinylabproductions.TLPLib.Test {
  public class TestLogger : LogBase {
    public readonly List<LogEntry>
      verboseMsgs = new List<LogEntry>(),
      debugMsgs = new List<LogEntry>(),
      infoMsgs = new List<LogEntry>(),
      warnMsgs = new List<LogEntry>(),
      errorMsgs = new List<LogEntry>();

    public int count =>
      verboseMsgs.Count + debugMsgs.Count + infoMsgs.Count + warnMsgs.Count + errorMsgs.Count;

    public bool isEmpty => count == 0;
    public bool nonEmpty => !isEmpty;

    public bool errorsAsExceptions;

    public TestLogger(bool errorsAsExceptions = false) {
      this.errorsAsExceptions = errorsAsExceptions;
      level = LogLevel.VERBOSE;
    }

    public void clear() {
      verboseMsgs.Clear();
      debugMsgs.Clear();
      infoMsgs.Clear();
      warnMsgs.Clear();
      errorMsgs.Clear();
    }

    protected override void logInner(LogLevel l, LogEntry entry) {
      switch (l) {
        case LogLevel.ERROR:
          if (errorsAsExceptions) throw new Exception(entry.ToString());
          errorMsgs.Add(entry);
          break;
        case LogLevel.WARN:
          warnMsgs.Add(entry);
          break;
        case LogLevel.INFO:
          infoMsgs.Add(entry);
          break;
        case LogLevel.DEBUG:
          debugMsgs.Add(entry);
          break;
        case LogLevel.VERBOSE:
          verboseMsgs.Add(entry);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(l), l, null);
      }
    }
  }
}
