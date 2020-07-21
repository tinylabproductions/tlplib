using pzd.lib.log;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Logger {
  public class ScopedLog : ILog {
    public readonly string scope;
    readonly ILog backing;

    public ScopedLog(string scope, ILog backing) {
      this.scope = scope;
      this.backing = backing;
    }

    public LogLevel level {
      get { return backing.level; }
      set { backing.level = value; }
    }

    public IRxObservable<LogEvent> messageLogged => backing.messageLogged;

    public bool willLog(LogLevel l) => backing.willLog(l);
    public void log(LogLevel l, LogEntry entry) =>
      backing.log(l, entry.withMessage(wrap(entry.message)));

    string wrap(object o) => $"[{scope}] {o}";
  }

  public static class ScopedLogExts {
    public static ILog withScope(this ILog log, string scope) => new ScopedLog(scope, log);
  }
}