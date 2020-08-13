using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.dispose;
using pzd.lib.functional;
using pzd.lib.log;
using pzd.lib.reactive;

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

    [PublicAPI] public delegate void OnError(LogEvent data);

    [PublicAPI] public static readonly LazyVal<IRxObservable<LogEvent>> defaultStream =
      UnityLog.fromUnityLogMessages.lazyMap(o => o.join(Log.@default.messageLogged));

    /// <summary>
    /// Report warnings and errors from default logger and unity log messages.
    /// </summary>
    [PublicAPI] 
    public static ISubscription registerDefault(
      this OnError onError, IDisposableTracker tracker, LogLevel logFrom
    ) =>
      defaultStream.strict
      .filter(e => e.entry.reportToErrorTracking && e.level >= logFrom)
      .subscribe(tracker, e => onError(e));
  }
}
