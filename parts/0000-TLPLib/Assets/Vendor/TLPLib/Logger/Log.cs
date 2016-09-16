using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Threads;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.tinylabproductions.TLPLib.Logger {
  /**
   * This double checks logging levels because string concatenation is more 
   * expensive than boolean check.
   *
   * The general rule of thumb is that if your log object doesn't need any 
   * processing you can call appropriate logging method by itself. If it does
   * need processing, you should use `if (Log.isDebug) Log.rdebug("foo=" + foo);` style.
   **/
  public static class Log {
    public enum Level : byte { NONE, ERROR, WARN, INFO, DEBUG, VERBOSE }

    public static readonly Level defaultLogLevel = 
      Application.isEditor || Debug.isDebugBuild ? Level.DEBUG : Level.INFO;

    public static ILog defaultLogger => UnityLog.instance;

    static Log() {
      DConsole.instance.onShow += dc => {
        var r = dc.registrarFor("Default Logger");
        r.register("level?", () => defaultLogger.level);
        foreach (var l in EnumUtils.GetValues<Level>())
          r.register($"level={l}", () => defaultLogger.level = l);
      };
    }

    public static void verbose(object o) { UnityLog.instance.verbose(o); }
    public static bool isVerbose => UnityLog.instance.isVerbose;
    
    /* Runtime version of debug. */
    public static void rdebug(object o) { UnityLog.instance.debug(o); }
    public static bool isDebug => UnityLog.instance.isDebug;

    public static void info(object o) { UnityLog.instance.info(o); }
    public static bool isInfo => UnityLog.instance.isInfo;

    public static void warn(object o) { UnityLog.instance.warn(o); }
    public static bool isWarn => UnityLog.instance.isWarn;

    public static void error(Exception ex) { UnityLog.instance.error(ex); }
    public static void error(object o) { UnityLog.instance.error(o); }
    public static void error(object o, Exception ex) { UnityLog.instance.error(o, ex); }
    public static bool isError => UnityLog.instance.isError;

    public static string exToStr(Exception ex, object o=null) {
      var sb = new StringBuilder();
      if (o != null) sb.AppendLine(o.ToString());
      sb.AppendLine(exToStrOneLine(ex));
      var stacktrace = ex.StackTrace;

      ex = ex.InnerException;
      while (ex != null)
      {
        sb.AppendLine($"Caused by {exToStrOneLine(ex)}");
        ex = ex.InnerException;
      }
      sb.AppendLine(stacktrace);
      return sb.ToString();
    }

    static string exToStrOneLine(Exception ex) { return $"{ex.GetType()}: {ex.Message}"; }

    [Conditional("UNITY_EDITOR")]
    public static void editor(object o) { EditorLog.log(o); }

    public static Option<Level> levelFromString(string s) {
      switch (s) {
        case "debug": return F.some(Level.DEBUG);
        case "info": return F.some(Level.INFO);
        case "warn": return F.some(Level.WARN);
        case "error": return F.some(Level.ERROR);
        case "none": return F.some(Level.NONE);
        default: return F.none<Level>();
      }
    }
  }

  public interface ILog {
    Log.Level level { get; set; }
    bool willLog(Log.Level level);
    void verbose(object o);
    bool isVerbose { get; }
    /* Runtime version of debug. */
    void debug(object o);
    bool isDebug { get; }
    void info(object o);
    bool isInfo { get; }
    void warn(object o);
    bool isWarn { get; }
    void error(Exception ex);
    void error(object o);
    void error(object o, Exception ex);
  }

  /**
   * Useful for logging from inside Application.logMessageReceivedThreaded, because 
   * log calls are silently ignored from inside the handlers. Just make sure not to 
   * get into an endless loop.
   **/
  public class DeferToMainThreadLog : ILogMethods {
    readonly ILog log;

    public DeferToMainThreadLog(ILog log) { this.log = log; }

    public override Log.Level level {
      get { return log.level; }
      set { log.level = value; }
    }

    public override bool willLog(Log.Level level) => log.willLog(level);
    public override void verbose(object o) => defer(() => log.verbose(o));
    public override void debug(object o) => defer(() => log.debug(o));
    public override void info(object o) => defer(() => log.info(o));
    public override void warn(object o) => defer(() => log.warn(o));
    public override void error(object o) => defer(() => log.error(o));

    void defer(Action a) => ASync.OnMainThread(a, runNowIfOnMainThread: false);
  }

  public static class ILogExts {
    /* Backwards compatibility */
    [Obsolete("Use debug() instead.")]
    public static void rdebug(this ILog log, object o) { log.debug(o); }
  }

  public abstract class ILogMethods : ILog {
    public abstract Log.Level level { get; set; }
    public abstract bool willLog(Log.Level level);
    public abstract void verbose(object o);
    public abstract void debug(object o);
    public abstract void info(object o);
    public abstract void warn(object o);
    public abstract void error(object o);

    public bool isVerbose => willLog(Log.Level.VERBOSE);
    public bool isDebug => willLog(Log.Level.DEBUG);
    public bool isInfo => willLog(Log.Level.INFO);
    public bool isWarn => willLog(Log.Level.WARN);
    public bool isError => willLog(Log.Level.ERROR);

    public void error(Exception ex) => error(Log.exToStr(ex));
    public void error(object o, Exception ex) => error(Log.exToStr(ex, o));
  }

  public abstract class LogBase : ILogMethods {
    public override Log.Level level { get; set; } = Log.defaultLogLevel;
    public override bool willLog(Log.Level level) => this.level >= level;

    public override void verbose(object o) => logVerbose(line("VERBOSE", o));
    protected abstract void logVerbose(string s);

    public override void debug(object o) => logDebug(line("DEBUG", o));
    protected abstract void logDebug(string s);

    public override void info(object o) => logInfo(line("INFO", o));
    protected abstract void logInfo(string s);

    public override void warn(object o) => logWarn(line("WARN", o));
    protected abstract void logWarn(string s);

    public override void error(object o) => logError(line("ERROR", o));
    protected abstract void logError(string s);

    static string line(string level, object o) => $"[{thread}|{level}]> {o}";

    static string thread { get {
      var t = Thread.CurrentThread;
      return t == OnMainThread.mainThread ? "Tm" : $"T{t.ManagedThreadId}";
    } }
  }

  public class UnityLog : LogBase {
    public static readonly UnityLog instance = new UnityLog();
    UnityLog() {}

    protected override void logVerbose(string s) { Debug.Log(s); }
    protected override void logDebug(string s) { Debug.Log(s); }
    protected override void logInfo(string s) { Debug.Log(s); }
    protected override void logWarn(string s) { Debug.LogWarning(s); }
    protected override void logError(string s) { Debug.LogError(s); }
  }

  public class NoOpLog : LogBase {
    public static readonly NoOpLog instance = new NoOpLog();
    NoOpLog() {}

    protected override void logVerbose(string s) {}
    protected override void logDebug(string s) {}
    protected override void logInfo(string s) {}
    protected override void logWarn(string s) {}
    protected override void logError(string s) {}
  }

  class EditorLog {
    public static readonly string logfilePath;
    public static readonly StreamWriter logfile;
    
    static EditorLog() {
      logfilePath = Application.temporaryCachePath + "/unity-editor-runtime.log";
      Log.info("Editor Runtime Logfile: " + logfilePath);
      logfile = new StreamWriter(
        File.Open(logfilePath, FileMode.Append, FileAccess.Write, FileShare.Read)
      ) { AutoFlush = true };

      log("\n\nLog opened at " + DateTime.Now + "\n\n");
    }

    [Conditional("UNITY_EDITOR")]
    public static void log(object o) { logfile.WriteLine(o); }
  }
}
