﻿using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;

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

    public static readonly LazyVal<IObservable<LogEvent>> defaultStream =
      UnityLog.fromUnityLogMessages.map(o => o.join(Log.@default.messageLogged));

    /// <summary>
    /// Report warnings and errors from default logger and unity log messages.
    /// </summary>
    public static ISubscription registerDefault(
      this OnError onError, Log.Level logFrom = Log.Level.WARN
    ) => defaultStream.get.filter(e => e.level >= logFrom).subscribe(e => onError(e));
  }
}
