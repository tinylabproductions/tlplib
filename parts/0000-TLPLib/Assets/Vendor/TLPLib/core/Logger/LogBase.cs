using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Threads;

namespace com.tinylabproductions.TLPLib.Logger {
  public abstract class LogBase : ILog {
    readonly ISubject<LogEvent> _messageLogged = new Subject<LogEvent>();
    public IRxObservable<LogEvent> messageLogged => _messageLogged;
    // Can't use Unity time, because it is not thread safe
    static readonly DateTime initAt = DateTime.Now;

    public Log.Level level { get; set; } = Log.defaultLogLevel;
    public bool willLog(Log.Level l) => l >= level;

    public void log(Log.Level l, LogEntry entry) {
      logInner(l, entry.withMessage(line(l.ToString(), entry.message)));
      var logEvent = new LogEvent(l, entry);
      if (OnMainThread.isMainThread) _messageLogged.push(logEvent);
      else {
        // extracted method to avoid closure allocation when running on main thread
        logOnMainThread(logEvent);
      }
    }

    void logOnMainThread(LogEvent logEvent) => OnMainThread.run(() => _messageLogged.push(logEvent));

    protected abstract void logInner(Log.Level l, LogEntry entry);

    static string line(string level, object o) => $"[{(DateTime.Now - initAt).TotalSeconds:F3}|{thread}|{level}]> {o}";

    static string thread => (OnMainThread.isMainThread ? "Tm" : "T") + Thread.CurrentThread.ManagedThreadId;
  }
}