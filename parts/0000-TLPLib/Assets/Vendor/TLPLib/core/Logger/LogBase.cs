using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Reactive;
using com.tinylabproductions.TLPLib.Threads;
using pzd.lib.log;
using pzd.lib.reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger {
  public abstract class LogBase : ILog {
    readonly ISubject<LogEvent> _messageLogged = new Subject<LogEvent>();
    public IRxObservable<LogEvent> messageLogged => _messageLogged;
    // Can't use Unity time, because it is not thread safe
    static readonly DateTime initAt = DateTime.Now;

    public LogLevel level { get; set; } = Log.defaultLogLevel;
    public bool willLog(LogLevel l) => l >= level;

    public void log(LogLevel l, LogEntry entry) {
      logInner(l, entry.withMessage(line(l.ToString(), entry.message)));
      var logEvent = new LogEvent(l, entry);
      if (OnMainThread.isMainThread) _messageLogged.push(logEvent);
      else {
        // extracted method to avoid closure allocation when running on main thread
        logOnMainThread(logEvent);
      }
    }

    void logOnMainThread(LogEvent logEvent) => OnMainThread.run(() => _messageLogged.push(logEvent));

    protected abstract void logInner(LogLevel l, LogEntry entry);

    static string line(string level, object o) => 
      $"[{(DateTime.Now - initAt).TotalSeconds:F3}|{thread}|{frame}|{level}]> {o}";

    static string thread => (OnMainThread.isMainThread ? "Tm" : "T") + Thread.CurrentThread.ManagedThreadId;
    static string frame => (OnMainThread.isMainThread ? "f" + Time.frameCount : "f-");
  }
}