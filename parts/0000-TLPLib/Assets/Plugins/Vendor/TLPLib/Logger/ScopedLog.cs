namespace com.tinylabproductions.TLPLib.Logger {
  public class ScopedLog : ILog {
    public readonly string scope;
    readonly ILog backing;

    public ScopedLog(string scope, ILog backing) {
      this.scope = scope;
      this.backing = backing;
    }

    public Log.Level level {
      get { return backing.level; }
      set { backing.level = value; }
    }

    public bool willLog(Log.Level l) => backing.willLog(l);
    public void log(Log.Level l, object o) => backing.log(l, wrap(o));

    string wrap(object o) => $"{scope} {o}";
  }

  public static class ScopedLogExts {
    public static ILog withScope(this ILog log, string scope) => new ScopedLog(scope, log);
  }
}