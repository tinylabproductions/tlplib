using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Test {
  public class TestLogger : LogBase {
    public readonly List<string> 
      verboseMsgs = new List<string>(), 
      debugMsgs = new List<string>(), 
      infoMsgs = new List<string>(),
      warnMsgs = new List<string>(),
      errorMsgs = new List<string>();

    public TestLogger() {
      level = Log.Level.VERBOSE; 
    }

    public void clear() {
      verboseMsgs.Clear();
      debugMsgs.Clear();
      infoMsgs.Clear();
      warnMsgs.Clear();
      errorMsgs.Clear();
    }

    protected override void logInner(Log.Level l, string s) {
      switch (l) {
        case Log.Level.NONE:
          break;
        case Log.Level.ERROR:
          errorMsgs.Add(s);
          break;
        case Log.Level.WARN:
          warnMsgs.Add(s);
          break;
        case Log.Level.INFO:
          infoMsgs.Add(s);
          break;
        case Log.Level.DEBUG:
          debugMsgs.Add(s);
          break;
        case Log.Level.VERBOSE:
          verboseMsgs.Add(s);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(l), l, null);
      }
    }
  }
}
