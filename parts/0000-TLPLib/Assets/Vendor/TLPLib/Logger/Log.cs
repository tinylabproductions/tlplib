using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using com.tinylabproductions.TLPLib.Functional;
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
    public enum Level { NONE, ERROR, WARN, INFO, DEBUG }

    public static readonly Level defaultLogLevel = 
      Application.isEditor || Debug.isDebugBuild ? Level.DEBUG : Level.INFO;

    public static ILog defaultLogger => UnityLog.instance;

    /* Compile time version of debug. */
    [Conditional("UNITY_EDITOR"), Conditional("LOG_DEBUG")]
    public static void cdebug(object o) { rdebug(o); }

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

  public static class ILogExts {
    /* Backwards compatibility */
    [Obsolete("Use debug() instead.")]
    public static void rdebug(this ILog log, object o) { log.debug(o); }
  }

  public abstract class LogBase : ILog {
    public Log.Level level { get; set; } = Log.defaultLogLevel;

    public bool isDebug => level >= Log.Level.DEBUG;
    public void debug(object o) { if (isDebug) logDebug($"[DEBUG]> {o}"); }
    protected abstract void logDebug(string s);

    public bool isInfo => level >= Log.Level.INFO;
    public void info(object o) { if (isInfo) logInfo($"[INFO]> {o}"); }
    protected abstract void logInfo(string s);

    public bool isWarn => level >= Log.Level.WARN;
    public void warn(object o) { if (isWarn) logWarn($"[WARN]> {o}"); }
    protected abstract void logWarn(string s);

    public bool isError => level >= Log.Level.ERROR;
    public void error(Exception ex) { error(Log.exToStr(ex)); }
    public void error(object o, Exception ex) { error(Log.exToStr(ex, o)); }
    public void error(object o) { logError($"[ERROR]> {o}"); }
    protected abstract void logError(string s);
  }

  public class UnityLog : LogBase {
    public static readonly UnityLog instance = new UnityLog();
    UnityLog() {}

    protected override void logDebug(string s) { Debug.Log(s); }
    protected override void logInfo(string s) { Debug.Log(s); }
    protected override void logWarn(string s) { Debug.LogWarning(s); }
    protected override void logError(string s) { Debug.LogError(s); }
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
