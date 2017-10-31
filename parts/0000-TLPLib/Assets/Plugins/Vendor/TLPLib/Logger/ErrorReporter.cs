using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger {
  public static class ErrorReporter {
    public struct AppInfo {
      public readonly string bundleIdentifier, productName;
      public readonly VersionNumber bundleVersion;

      public AppInfo(string bundleIdentifier, VersionNumber bundleVersion, string productName) {
        this.bundleIdentifier = bundleIdentifier;
        this.bundleVersion = bundleVersion;
        this.productName = productName;
      }
    }

    public delegate void OnError(LogEvent data);

    public static ISubscription registerToILog(
      this OnError onError, Log.Level logFrom = Log.Level.WARN, ILog logger = null
    ) {
      logger = logger ?? Log.@default;
      return logger.messageLogged.filter(e => e.level >= logFrom).subscribe(e => onError(e));
    }

    public static void registerToUnity(OnError onError, bool logWarnings) {
      Application.logMessageReceivedThreaded += (message, backtrace, type) => {
        if (
          type == LogType.Assert || type == LogType.Error || type == LogType.Exception
          || logWarnings && type == LogType.Warning
        ) {
          try {
            // We want to collect backtrace on the current thread
            var parsedBacktrace =
              // backtrace may be empty in release mode.
              string.IsNullOrEmpty(backtrace)
                ? BacktraceElem.generateFromHere(1)
                : BacktraceElem.parseUnityBacktrace(backtrace);
            var data = new ErrorData(type, message, parsedBacktrace);
            // But call our error handler on main thread
            // because handlers are not guaranteed to be thread safe
            // and Log.d.info would not work in our handler
            ASync.OnMainThread(
              () => {
                try { onError(data); }
                catch (Exception e) { logExceptionSafe(e); }
              },
              runNowIfOnMainThread: false
            );
          }
          catch (Exception e) { logExceptionSafe(e); }
        }
      };
    }

    static void logExceptionSafe(Exception e) => ASync.OnMainThread(
      () => {
        // Log at info level so that we wouldn't trigger this handler again.
        Log.d.info(
          $"[{nameof(ErrorReporter)}] Exception in " +
          $"{nameof(Application)}.{nameof(Application.logMessageReceivedThreaded)}" +
          $" handler!\n\n{e}"
        );
      },
      // https://fogbugz.unity3d.com/default.asp?832198_48nbh0a3a8cjpr12
      runNowIfOnMainThread: false
    );
  }
}
