using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using com.tinylabproductions.TLPLib.Components.DebugConsole;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Threads;
using com.tinylabproductions.TLPLib.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

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
      Application.isEditor || Debug.isDebugBuild
      ? Level.DEBUG : Level.INFO;

    static readonly bool useConsoleLog = EditorUtils.inBatchMode;

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

    static ILog _defaultLogger;
    public static ILog defaultLogger {
      get {
        return _defaultLogger ?? (
          _defaultLogger = useConsoleLog ? (ILog) ConsoleLog.instance : UnityLog.instance
        );
      }
      set { _defaultLogger = value; }
    }

    public static void log(Level l, object o, Object context = null) => 
      defaultLogger.log(l, LogEntry.simple(o, context));
    public static void log(Level l, LogEntry entry) => defaultLogger.log(l, entry);
    public static bool willLog(Level l) => defaultLogger.willLog(l);

    public static void verbose(object o, Object context = null) => 
      defaultLogger.verbose(o, context);
    public static bool isVerbose => defaultLogger.isVerbose();
    
    /* Runtime version of debug. */
    public static void rdebug(object o, Object context = null) => 
      defaultLogger.debug(o, context);
    public static bool isDebug => defaultLogger.isDebug();

    public static void info(object o, Object context = null) => 
      defaultLogger.info(o, context);
    public static bool isInfo => defaultLogger.isInfo();

    public static void warn(object o, Object context = null) => 
      defaultLogger.warn(o, context);
    public static bool isWarn => defaultLogger.isWarn();

    public static void error(Exception ex, Object context = null) => 
      defaultLogger.error(ex, context);
    public static void error(object o, Object context = null) => 
      defaultLogger.error(o, context);
    public static void error(object o, Exception ex, Object context = null) => 
      defaultLogger.error(o, ex, context);
    public static bool isError => defaultLogger.isError();

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

  public struct LogEntry {
    /// <summary>Message for the log entry.</summary>
    public readonly object message;
    /// <summary>key -> value pairs where values make up a set. Things like
    /// type -> (dog or cat or fish) are a good fit here.</summary>
    public readonly ImmutableArray<Tpl<string, string>> tags;
    /// <summary>
    /// key -> value pairs where values can be anything. Things like
    /// bytesRead -> 322344 are a good fit here.
    /// </summary>
    public readonly ImmutableArray<Tpl<string, string>> extras;
    /// <summary>Unity object which is related to this entry.</summary>
    public readonly Option<Object> context;

    public LogEntry(
      object message, ImmutableArray<Tpl<string, string>> tags, 
      ImmutableArray<Tpl<string, string>> extras,
      Option<Object> context = default(Option<Object>)
    ) {
      Option.ensureValue(ref context);
      this.message = message;
      this.tags = tags;
      this.extras = extras;
      this.context = context;
    }

    public override string ToString() {
      var sb = new StringBuilder(message.ToString());
      if (context.isSome) sb.Append($" (ctx={context.__unsafeGetValue})");
      if (tags.nonEmpty()) sb.Append($"\n{nameof(tags)}={tags.mkStringEnumNewLines()}");
      if (extras.nonEmpty()) sb.Append($"\n{nameof(extras)}={extras.mkStringEnumNewLines()}");
      return sb.ToString();
    }

    public static LogEntry simple(object message, Object context = null) => new LogEntry(
      message, ImmutableArray<Tpl<string, string>>.Empty, 
      ImmutableArray<Tpl<string, string>>.Empty, context.opt()
    );

    public LogEntry withMessage(string message) => 
      new LogEntry(message, tags, extras, context);

    public static readonly ISerializedRW<ImmutableArray<Tpl<string, string>>> kvArraySerializedRw =
      SerializedRW.immutableArray(SerializedRW.str.and(SerializedRW.str));
  }

  public interface ILog {
    Log.Level level { get; set; }

    bool willLog(Log.Level l);
    void log(Log.Level l, LogEntry o);
  }

  public static class ILogExts {
    public static bool isVerbose(this ILog log) => log.willLog(Log.Level.VERBOSE);
    public static bool isDebug(this ILog log) => log.willLog(Log.Level.DEBUG);
    public static bool isInfo(this ILog log) => log.willLog(Log.Level.INFO);
    public static bool isWarn(this ILog log) => log.willLog(Log.Level.WARN);
    public static bool isError(this ILog log) => log.willLog(Log.Level.ERROR);

    public static void verbose(this ILog log, object o, Object context = null) => 
      log.log(Log.Level.VERBOSE, LogEntry.simple(o, context));
    public static void debug(this ILog log, object o, Object context = null) => 
      log.log(Log.Level.DEBUG, LogEntry.simple(o, context));
    public static void info(this ILog log, object o, Object context = null) => 
      log.log(Log.Level.INFO, LogEntry.simple(o, context));
    public static void warn(this ILog log, object o, Object context = null) => 
      log.warn(LogEntry.simple(o, context));
    public static void warn(this ILog log, LogEntry entry) => 
      log.log(Log.Level.WARN, entry);
    public static void error(this ILog log, object o, Object context = null) => 
      log.error(LogEntry.simple(o, context));
    public static void error(this ILog log, LogEntry entry) => 
      log.log(Log.Level.ERROR, entry);
    public static void error(this ILog log, Exception ex, Object context = null) => 
      log.error(Log.exToStr(ex), context);
    public static void error(this ILog log, object o, Exception ex, Object context = null) => 
      log.error(Log.exToStr(ex, o), context);

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
    public void log(Log.Level l, LogEntry entry) => 
      defer(() => backing.log(l, entry));

    static void defer(Action a) => ASync.OnMainThread(a, runNowIfOnMainThread: false);
  }
  
  public abstract class LogBase : ILog {
    public Log.Level level { get; set; } = Log.defaultLogLevel;
    public bool willLog(Log.Level l) => level >= l;

    public void log(Log.Level l, LogEntry entry) => 
      logInner(l, entry.withMessage(line(l.ToString(), entry.message)));
    protected abstract void logInner(Log.Level l, LogEntry entry);

    static string line(string level, object o) => $"[{thread}|{level}]> {o}";

    static string thread { get {
      var t = Thread.CurrentThread;
      return t == OnMainThread.mainThread ? "Tm" : $"T{t.ManagedThreadId}";
    } }
  }

  /** Useful for batch mode to log to the log file without the stacktraces. */
  public class ConsoleLog : LogBase {
    public static readonly ConsoleLog instance = new ConsoleLog();
    ConsoleLog() {}

    protected override void logInner(Log.Level l, LogEntry entry) => 
      Console.WriteLine(entry.ToString());
  }

  public class UnityLog : LogBase {
    public static readonly UnityLog instance = new UnityLog();
    UnityLog() {}

    protected override void logInner(Log.Level l, LogEntry entry) {
      switch (l) {
        case Log.Level.VERBOSE:
        case Log.Level.DEBUG:
        case Log.Level.INFO:
          Debug.Log(entry, entry.context.getOrNull());
          break;
        case Log.Level.WARN:
          Debug.LogWarning(entry, entry.context.getOrNull());
          break;
        case Log.Level.ERROR:
          Debug.LogError(entry, entry.context.getOrNull());
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

    protected override void logInner(Log.Level l, LogEntry entry) {}
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
