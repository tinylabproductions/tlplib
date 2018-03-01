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
using com.tinylabproductions.TLPLib.Reactive;
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
   * need processing, you should use `if (Log.d.isDebug()) Log.d.debug("foo=" + foo);` style.
   **/
  public static class Log {
    public enum Level : byte { VERBOSE = 10, DEBUG = 20, INFO = 30, WARN = 40, ERROR = 50 }
    public static class Level_ {
      public static readonly ISerializedRW<Level> rw = SerializedRW.byte_.map(
        b => ((Level) b).some(),
        l => (byte) l
      );

      public static readonly ISerializedRW<Option<Level>> optRw = SerializedRW.opt(rw);
    }

    public static readonly Level defaultLogLevel =
      Application.isEditor || Debug.isDebugBuild
      ? Level.DEBUG : Level.INFO;

    static readonly bool useConsoleLog = EditorUtils.inBatchMode;

    static Log() {
      DConsole.instance.onShow += dc => {
        var r = dc.registrarFor("Default Logger");
        r.registerEnum(
          "level",
          Ref.a(() => @default.level, v => @default.level = v),
          EnumUtils.GetValues<Level>()
        );
      };
    }

    static ILog _default;
    public static ILog @default {
      get {
        return _default ?? (
          _default = useConsoleLog ? (ILog) ConsoleLog.instance : UnityLog.instance
        );
      }
      set { _default = value; }
    }

    /// <summary>
    /// Shorthand for <see cref="Log.@default"/>. Allows <code><![CDATA[
    /// if (Log.d.isInfo) Log.d.info("foo");
    /// ]]></code> syntax.
    /// </summary>
    public static ILog d => @default;

    [Conditional("UNITY_EDITOR")]
    public static void editor(object o) => EditorLog.log(o);
  }

  public struct LogEntry {
    /// <summary>Message for the log entry.</summary>
    public readonly string message;
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
    /// <summary>A log entry might have backtrace attached to it.</summary>
    public readonly Option<Backtrace> backtrace;

    public LogEntry(
      string message,
      ImmutableArray<Tpl<string, string>> tags,
      ImmutableArray<Tpl<string, string>> extras,
      Option<Backtrace> backtrace = default(Option<Backtrace>),
      Option<Object> context = default(Option<Object>)
    ) {
      Option.ensureValue(ref backtrace);
      Option.ensureValue(ref context);

      this.message = message;
      this.tags = tags;
      this.extras = extras;
      this.backtrace = backtrace;
      this.context = context;
    }

    public override string ToString() {
      var sb = new StringBuilder(message);
      if (context.isSome) sb.Append($" (ctx={context.__unsafeGetValue})");
      if (tags.nonEmpty()) sb.Append($"\n{nameof(tags)}={tags.mkStringEnumNewLines()}");
      if (extras.nonEmpty()) sb.Append($"\n{nameof(extras)}={extras.mkStringEnumNewLines()}");
      if (backtrace.isSome) sb.Append($"\n{backtrace.__unsafeGetValue}");
      return sb.ToString();
    }

    public static LogEntry simple(
      string message, Option<Backtrace> backtrace = default(Option<Backtrace>), Object context = null
    ) => new LogEntry(
      message, ImmutableArray<Tpl<string, string>>.Empty,
      ImmutableArray<Tpl<string, string>>.Empty,
      backtrace: backtrace, context: context.opt()
    );

    public static LogEntry fromException(
      string message, Exception ex, Object context = null
    ) => simple($"{message}: {ex.Message}", Backtrace.fromException(ex), context);

    public LogEntry withMessage(string message) =>
      new LogEntry(message, tags, extras, backtrace, context);

    public LogEntry withMessage(Fn<string, string> message) =>
      new LogEntry(message(this.message), tags, extras, backtrace, context);

    public static readonly ISerializedRW<ImmutableArray<Tpl<string, string>>> kvArraySerializedRw =
      SerializedRW.immutableArray(SerializedRW.str.and(SerializedRW.str));
  }

  public struct LogEvent {
    public readonly Log.Level level;
    public readonly LogEntry entry;

    public LogEvent(Log.Level level, LogEntry entry) {
      this.level = level;
      this.entry = entry;
    }

    public override string ToString() => $"{nameof(LogEvent)}[{level}, {entry}]";
  }

  public interface ILog {
    Log.Level level { get; set; }

    bool willLog(Log.Level l);
    void log(Log.Level l, LogEntry o);
    IObservable<LogEvent> messageLogged { get; }
  }

  public static class ILogExts {
    public static bool isVerbose(this ILog log) => log.willLog(Log.Level.VERBOSE);
    public static bool isDebug(this ILog log) => log.willLog(Log.Level.DEBUG);
    public static bool isInfo(this ILog log) => log.willLog(Log.Level.INFO);
    public static bool isWarn(this ILog log) => log.willLog(Log.Level.WARN);

    public static void log(this ILog log, Log.Level l, string message) =>
      log.log(l, LogEntry.simple(message));

    public static void verbose(this ILog log, string msg, Object context = null) =>
      log.log(Log.Level.VERBOSE, LogEntry.simple(msg, context: context));
    public static void debug(this ILog log, string msg, Object context = null) =>
      log.log(Log.Level.DEBUG, LogEntry.simple(msg, context: context));
    public static void info(this ILog log, string msg, Object context = null) =>
      log.log(Log.Level.INFO, LogEntry.simple(msg, context: context));
    public static void warn(this ILog log, string msg, Object context = null) =>
      log.warn(LogEntry.simple(msg, context: context));
    public static void warn(this ILog log, LogEntry entry) =>
      log.log(Log.Level.WARN, entry);
    public static void error(this ILog log, string msg, Object context = null) =>
      log.error(LogEntry.simple(msg, context: context));
    public static void error(this ILog log, LogEntry entry) =>
      log.log(Log.Level.ERROR, entry);
    public static void error(this ILog log, Exception ex, Object context = null) =>
      log.error(ex.Message, ex, context);
    public static void error(this ILog log, string msg, Exception ex, Object context = null) =>
      log.error(LogEntry.fromException(msg, ex, context));
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

    public IObservable<LogEvent> messageLogged => backing.messageLogged;
  }

  public abstract class LogBase : ILog {
    readonly ISubject<LogEvent> _messageLogged = new Subject<LogEvent>();
    public IObservable<LogEvent> messageLogged => _messageLogged;

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

    static string line(string level, object o) => $"[{thread}|{level}]> {o}";

    static string thread => (OnMainThread.isMainThread ? "Tm" : "T") + Thread.CurrentThread.ManagedThreadId;
  }

  /** Useful for batch mode to log to the log file without the stacktraces. */
  public class ConsoleLog : LogBase {
    public static readonly ConsoleLog instance = new ConsoleLog();
    ConsoleLog() {}

    protected override void logInner(Log.Level l, LogEntry entry) =>
      Console.WriteLine(entry.ToString());
  }

  public class UnityLog : LogBase {
    /// <summary>
    /// Prefix to all messages so we could differentiate what comes from
    /// our logging framework in Unity console.
    /// </summary>
    public const string MESSAGE_PREFIX = "[TLPLog]";

    public static readonly UnityLog instance = new UnityLog();
    UnityLog() {}

    protected override void logInner(Log.Level l, LogEntry entry) {
      switch (l) {
        case Log.Level.VERBOSE:
        case Log.Level.DEBUG:
        case Log.Level.INFO:
          Debug.Log(s(entry), entry.context.getOrNull());
          break;
        case Log.Level.WARN:
          Debug.LogWarning(s(entry), entry.context.getOrNull());
          break;
        case Log.Level.ERROR:
          Debug.LogError(s(entry), entry.context.getOrNull());
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(l), l, null);
      }
    }

    static string s(LogEntry e) => $"{MESSAGE_PREFIX}{e}";

    static Try<LogEvent> convertUnityMessageToLogEvent(
      string message, string backtraceS, LogType type, int stackFramesToSkipWhenGenerating
    ) {
      try {
        var level = convertLevel(type);

        // We want to collect backtrace on the current thread
        var backtrace =
          level >= Log.Level.WARN
            ?
              // backtrace may be empty in release mode.
              string.IsNullOrEmpty(backtraceS)
                ? Backtrace.generateFromHere(stackFramesToSkipWhenGenerating + 1 /*this stack frame*/)
                : Backtrace.parseUnityBacktrace(backtraceS)
            : Option<Backtrace>.None;
        var logEvent = new LogEvent(level, new LogEntry(
          message,
          ImmutableArray<Tpl<string, string>>.Empty,
          ImmutableArray<Tpl<string, string>>.Empty,
          backtrace, context: Option<Object>.None
        ));
        return F.scs(logEvent);
      }
      catch (Exception e) {
        return F.err<LogEvent>(e);
      }
    }

    public static readonly LazyVal<IObservable<LogEvent>> fromUnityLogMessages = F.lazy(() => {
      var subject = new Subject<LogEvent>();
      Application.logMessageReceivedThreaded += (message, backtrace, type) => {
        // Ignore messages that we ourselves sent to Unity.
        if (message.StartsWithFast(MESSAGE_PREFIX)) return;
        var logEventTry = convertUnityMessageToLogEvent(
          message, backtrace, type,
          stackFramesToSkipWhenGenerating: 1 /* This stack frame */
        );
        var logEvent =
          logEventTry.isSuccess ? logEventTry.__unsafeGet
          : new LogEvent(Log.Level.ERROR, LogEntry.fromException(
            $"Error while converting Unity log message (type: {type}): {message}, backtrace: [{backtrace}]",
            logEventTry.__unsafeException
          ));

        ASync.OnMainThread(
          () => subject.push(logEvent),
          // Do not run code that might throw an exception itself in logMessageReceived.
          runNowIfOnMainThread: false
        );
      };
      return subject.asObservable();
    });

    static Log.Level convertLevel(LogType type) {
      switch (type) {
        case LogType.Error:
        case LogType.Exception:
        case LogType.Assert:
          return Log.Level.ERROR;
        case LogType.Warning:
          return Log.Level.WARN;
        case LogType.Log:
          return Log.Level.INFO;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
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
      if (Log.d.isInfo()) Log.d.info("Editor Runtime Logfile: " + logfilePath);
      logfile = new StreamWriter(
        File.Open(logfilePath, FileMode.Append, FileAccess.Write, FileShare.Read)
      ) { AutoFlush = true };

      log("\n\nLog opened at " + DateTime.Now + "\n\n");
    }

    [Conditional("UNITY_EDITOR")]
    public static void log(object o) { logfile.WriteLine(o); }
  }
}
