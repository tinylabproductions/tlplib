namespace com.tinylabproductions.TLPLib.Logger {
  public class ScopedLog : LogBase {
    public readonly string scope;
    readonly ILog backing;

    public ScopedLog(string scope, ILog backing) {
      this.scope = scope;
      this.backing = backing;
    }

    public override void verbose(object o) => base.verbose(wrap(o));
    public override void debug(object o) => base.debug(wrap(o));
    public override void info(object o) => base.info(wrap(o));
    public override void warn(object o) => base.warn(wrap(o));
    public override void error(object o) => base.error(wrap(o));

    string wrap(object o) => $"{scope} {o}";

    protected override void logVerbose(string s) => backing.verbose(s);
    protected override void logDebug(string s) => backing.debug(s);
    protected override void logInfo(string s) => backing.info(s);
    protected override void logWarn(string s) => backing.warn(s);
    protected override void logError(string s) => backing.error(s);
  }

  public static class ScopedLogExts {
    public static ILog withScope(this ILog log, string scope) => new ScopedLog(scope, log);
  }
}