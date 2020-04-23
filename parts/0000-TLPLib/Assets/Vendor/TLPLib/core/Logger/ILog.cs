using System;
using com.tinylabproductions.TLPLib.Reactive;
using GenerationAttributes;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Logger {
  public interface ILog {
    Log.Level level { get; set; }

    bool willLog(Log.Level l);
    void log(Log.Level l, LogEntry o);
    IRxObservable<LogEvent> messageLogged { get; }
  }

  [PublicAPI] public static class ILogExts {
    public static bool isVerbose(this ILog log) => log.willLog(Log.Level.VERBOSE);
    public static bool isDebug(this ILog log) => log.willLog(Log.Level.DEBUG);
    public static bool isInfo(this ILog log) => log.willLog(Log.Level.INFO);
    public static bool isWarn(this ILog log) => log.willLog(Log.Level.WARN);

    public static void log(this ILog log, Log.Level l, string message) =>
      log.log(l, LogEntry.simple(message));

    [StatementMethodMacro(
@"{
  var macro__log__ = ${log};
  if (macro__log__.isVerbose()) macro__log__.verbose(msg: ${msg}, context: ${context});
}")]
    public static void mVerbose(this ILog log, string msg, object context = null) => throw new MacroException();
    public static void verbose(this ILog log, string msg, object context = null) =>
      log.log(Log.Level.VERBOSE, LogEntry.simple(msg, context: context));
    
    [StatementMethodMacro(
@"{
  var macro__log__ = ${log};
  if (macro__log__.isDebug()) macro__log__.debug(msg: ${msg}, context: ${context});
}")]
    public static void mDebug(this ILog log, string msg, object context = null) => throw new MacroException();
    public static void debug(this ILog log, string msg, object context = null) =>
      log.log(Log.Level.DEBUG, LogEntry.simple(msg, context: context));
    
    [StatementMethodMacro(
@"{
  var macro__log__ = ${log};
  if (macro__log__.isInfo()) macro__log__.info(msg: ${msg}, context: ${context});
}")]
    public static void mInfo(this ILog log, string msg, object context = null) => throw new MacroException();
    public static void info(this ILog log, string msg, object context = null) =>
      log.log(Log.Level.INFO, LogEntry.simple(msg, context: context));
    
    [StatementMethodMacro(
@"{
  var macro__log__ = ${log};
  if (macro__log__.isWarn()) macro__log__.warn(msg: ${msg}, context: ${context});
}")]
    public static void mWarn(this ILog log, string msg, object context = null) => throw new MacroException();
    public static void warn(this ILog log, string msg, object context = null) =>
      log.warn(LogEntry.simple(msg, context: context));
    public static void warn(this ILog log, LogEntry entry) =>
      log.log(Log.Level.WARN, entry);
    
    public static void error(this ILog log, string msg, object context = null) =>
      log.error(LogEntry.simple(msg, context: context));
    public static void error(this ILog log, LogEntry entry) =>
      log.log(Log.Level.ERROR, entry);
    public static void error(this ILog log, Exception ex, object context = null) =>
      log.error(ex.Message, ex, context);
    public static void error(this ILog log, string msg, Exception ex, object context = null) =>
      log.error(LogEntry.fromException(msg, ex, context: context));
    
    /// <summary>If success is false, logs the statement and returns.</summary>
    [StatementMethodMacro(
@"if (!${success}) {
  if (${log}.willLog(${level})) ${log}.log(${level}, ${msg});
  return;
}")]
    public static void outOr_LOG_AND_RETURN(
      this ILog log, bool success, LogEntry msg, Log.Level level
    ) => throw new MacroException();
  }
}