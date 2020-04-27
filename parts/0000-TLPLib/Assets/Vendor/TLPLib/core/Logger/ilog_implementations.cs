using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Logger {
  /**
   * Useful for logging from inside Application.logMessageReceivedThreaded, because
   * log calls are silently ignored from inside the handlers. Just make sure not to
   * get into an endless loop.
   **/
  [PublicAPI] public class DeferToMainThreadLog : ILog {
    readonly ILog backing;

    public DeferToMainThreadLog(ILog backing) { this.backing = backing; }

    public Log.Level level {
      get => backing.level;
      set => backing.level = value;
    }

    public bool willLog(Log.Level l) => backing.willLog(l);
    public void log(Log.Level l, LogEntry entry) =>
      defer(() => backing.log(l, entry));

    static void defer(Action a) => ASync.OnMainThread(a, runNowIfOnMainThread: false);

    public IRxObservable<LogEvent> messageLogged => backing.messageLogged;
  }

  /// <summary>
  /// Useful for batch mode to log to the log file without the stacktraces.
  /// </summary>
  [PublicAPI, Singleton] public partial class ConsoleLog : LogBase {
    protected override void logInner(Log.Level l, LogEntry entry) => Console.WriteLine(Str.s(entry));
  }

  [PublicAPI, Singleton] public partial class NoOpLog : LogBase {
    protected override void logInner(Log.Level l, LogEntry entry) {}
  }
}