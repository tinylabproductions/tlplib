using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger {
  public static class ErrorReporter {
    public delegate void OnError(LogType errorType, string message, Option<string> backtrace);

    static public void registerToUnity(OnError onError, bool logWarnings) {
      Application.logMessageReceivedThreaded += (message, backtrace, type) => {
        if (
          type == LogType.Assert || type == LogType.Error || type == LogType.Exception
          || (logWarnings && type == LogType.Warning)
        ) onError(
          type, message, (!string.IsNullOrEmpty(backtrace)).opt(backtrace)
        );
      };
    }
  }
}
