using System;
using System.Threading;
using com.tinylabproductions.TLPLib.Threads;
using pzd.lib.log;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Logger {
  public abstract class LogBase : BaseLog {
    // Can't use Unity time, because it is not thread safe
    static readonly DateTime initAt = DateTime.Now;

    protected LogBase() => level = Log.defaultLogLevel;

    protected override void logInternal(LogLevel l, LogEntry entry) {
      logInner(l, entry.withMessage(line(l.ToString(), entry.message)));
    }

    protected override void pushToMessageLogged(LogEvent e) {
      if (OnMainThread.isMainThread) _messageLogged.push(e);
      else {
        // extracted method to avoid closure allocation when running on main thread
        logOnMainThread(e);
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