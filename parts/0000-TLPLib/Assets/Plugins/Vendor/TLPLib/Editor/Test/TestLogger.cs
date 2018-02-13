using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;

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
      level = Log.Level.VERBOSE;
    }

    public void clear() {
      verboseMsgs.Clear();
      debugMsgs.Clear();
      infoMsgs.Clear();
      warnMsgs.Clear();
      errorMsgs.Clear();
    }

    protected override void logInner(Log.Level l, LogEntry entry) {
      switch (l) {
        case Log.Level.ERROR:
          if (errorsAsExceptions) throw new Exception(entry.ToString());
          errorMsgs.Add(entry);
          break;
        case Log.Level.WARN:
          warnMsgs.Add(entry);
          break;
        case Log.Level.INFO:
          infoMsgs.Add(entry);
          break;
        case Log.Level.DEBUG:
          debugMsgs.Add(entry);
          break;
        case Log.Level.VERBOSE:
          verboseMsgs.Add(entry);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(l), l, null);
      }
    }
  }
}
