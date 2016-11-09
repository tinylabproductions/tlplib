using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
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
        r.registerEnum(
          "level", 
          Ref.a(() => defaultLogger.level, v => defaultLogger.level = v),
          EnumUtils.GetValues<Level>()
        );
      };
    }

    public static void verbose(object o) { UnityLog.instance.verbose(o); }
    public static bool isVerbose => UnityLog.instance.isVerbose();
    
    /* Runtime version of debug. */
    public static void rdebug(object o) { UnityLog.instance.debug(o); }
    public static bool isDebug => UnityLog.instance.isDebug();

    public static void info(object o) { UnityLog.instance.info(o); }
    public static bool isInfo => UnityLog.instance.isInfo();

    public static void warn(object o) { UnityLog.instance.warn(o); }
    public static bool isWarn => UnityLog.instance.isWarn();

    public static void error(Exception ex) { UnityLog.instance.error(ex); }
    public static void error(object o) { UnityLog.instance.error(o); }
    public static void error(object o, Exception ex) { UnityLog.instance.error(o, ex); }
    public static bool isError => UnityLog.instance.isError();

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

    bool willLog(Log.Level l);
    void log(Log.Level l, object o);
  }

  public static class ILogExts {
    public static bool isVerbose(this ILog log) => log.willLog(Log.Level.VERBOSE);
    public static bool isDebug(this ILog log) => log.willLog(Log.Level.DEBUG);
    public static bool isInfo(this ILog log) => log.willLog(Log.Level.INFO);
    public static bool isWarn(this ILog log) => log.willLog(Log.Level.WARN);
    public static bool isError(this ILog log) => log.willLog(Log.Level.ERROR);

    public static void verbose(this ILog log, object o) => log.log(Log.Level.VERBOSE, o);
    public static void debug(this ILog log, object o) => log.log(Log.Level.DEBUG, o);
    public static void info(this ILog log, object o) => log.log(Log.Level.INFO, o);
    public static void warn(this ILog log, object o) => log.log(Log.Level.WARN, o);
    public static void error(this ILog log, object o) => log.log(Log.Level.ERROR, o);
    public static void error(this ILog log, Exception ex) => log.error(Log.exToStr(ex));
    public static void error(this ILog log, object o, Exception ex) => log.error(Log.exToStr(ex, o));

    /* Backwards compatibility */
    [Obsolete("Use debug() instead.")]
    public static void rdebug(this ILog log, object o) => log.debug(o);
  }

  /**
   * Useful for logging from inside Application.logMessageReceivedThreaded, because 
   * log calls are silently ignored from inside the handlers. Just make sure not to 
   * get into an endless loop.
   **/
  public class DeferToMainThreadLog : ILog {
    readonly ILog backing;

    public DeferToMainThreadLog(ILog backing) { this.backing = backing; }

    public Log.Level level {
      get { return backing.level; }
      set { backing.level = value; }
    }

    public bool willLog(Log.Level l) => backing.willLog(l);
    public void log(Log.Level l, object o) => defer(() => backing.log(l, o));

    static void defer(Action a) => ASync.OnMainThread(a, runNowIfOnMainThread: false);
  }
  
  public abstract class LogBase : ILog {
    public Log.Level level { get; set; } = Log.defaultLogLevel;
    public bool willLog(Log.Level l) => this.level >= l;

    public void log(Log.Level l, object o) => logInner(l, line(l.ToString(), o));
    protected abstract void logInner(Log.Level l, string s);

    static string line(string level, object o) => $"[{thread}|{level}]> {o}";

    static string thread { get {
      var t = Thread.CurrentThread;
      return t == OnMainThread.mainThread ? "Tm" : $"T{t.ManagedThreadId}";
    } }
  }

  public class UnityLog : LogBase {
    public static readonly UnityLog instance = new UnityLog();
    UnityLog() {}

    protected override void logInner(Log.Level l, string s) {
      switch (l) {
        case Log.Level.VERBOSE:
        case Log.Level.DEBUG:
        case Log.Level.INFO:
          Debug.Log(s);
          break;
        case Log.Level.WARN:
          Debug.LogWarning(s);
          break;
        case Log.Level.ERROR:
          Debug.LogError(s);
          break;
        case Log.Level.NONE:
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(l), l, null);
      }
    }
  }

  public class NoOpLog : LogBase {
    public static readonly NoOpLog instance = new NoOpLog();
    NoOpLog() {}

    protected override void logInner(Log.Level l, string s) {}
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
