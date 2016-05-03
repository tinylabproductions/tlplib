using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Logger;

namespace com.tinylabproductions.TLPLib.Test {
  public class TestLogger : LogBase {
    public readonly List<string> 
      debugMsgs = new List<string>(), 
      infoMsgs = new List<string>(),
      warnMsgs = new List<string>(),
      errorMsgs = new List<string>();

    public TestLogger() {
      level = Log.Level.DEBUG; 
    }

    public void clear() {
      debugMsgs.Clear();
      infoMsgs.Clear();
      warnMsgs.Clear();
      errorMsgs.Clear();
    }

    protected override void logDebug(string s) { debugMsgs.Add(s); }
    protected override void logInfo(string s) { infoMsgs.Add(s); }
    protected override void logWarn(string s) { warnMsgs.Add(s); }
    protected override void logError(string s) { errorMsgs.Add(s); }
  }
}
