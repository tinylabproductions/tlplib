using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Logger {
  public class LogTestDefaultLogLevel {
    [Test]
    public void DefaultLogLevelForDefaultLoggerShouldBeSet() {
      // This can fail if we have a circular dependency amongst static fields. C# silently 
      // ignores that value can't be resolved and assigns it to a default value. Yay!
      Log.defaultLogger.level.shouldEqual(Log.defaultLogLevel);
    }
  }
}