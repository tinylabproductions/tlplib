using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.reactive;

using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.log;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.tinylabproductions.TLPLib.Logger {
  public class UnityLog : LogBase {
    /// <summary>
    /// Prefix to all messages so we could differentiate what comes from
    /// our logging framework in Unity console.
    /// </summary>
    const string MESSAGE_PREFIX = "[TLPLog]";

    public static readonly UnityLog instance = new UnityLog();
    UnityLog() {}

    protected override void logInner(LogLevel l, LogEntry entry) {
      switch (l) {
        case LogLevel.VERBOSE:
        case LogLevel.DEBUG:
        case LogLevel.INFO:
          Debug.Log(s(entry), entry.maybeContext as Object);
          break;
        case LogLevel.WARN:
          Debug.LogWarning(s(entry), entry.maybeContext as Object);
          break;
        case LogLevel.ERROR:
          Debug.LogError(s(entry), entry.maybeContext as Object);
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
          level >= LogLevel.WARN
            ?
              // backtrace may be empty in release mode.
              string.IsNullOrEmpty(backtraceS)
                ? Backtrace.generateFromHere(stackFramesToSkipWhenGenerating + 1 /*this stack frame*/)
                : Backtrace.parseStringBacktrace(backtraceS, BacktraceElemUnity.parseBacktraceLine)
            : None._;
        var logEvent = new LogEvent(level, new LogEntry(
          message,
          ImmutableArray<KeyValuePair<string, string>>.Empty,
          ImmutableArray<KeyValuePair<string, string>>.Empty,
          reportToErrorTracking: true,
          backtrace: backtrace.toNullable(), context: null
        ));
        return F.scs(logEvent);
      }
      catch (Exception e) {
        return F.err<LogEvent>(e);
      }
    }

    public static readonly LazyVal<IRxObservable<LogEvent>> fromUnityLogMessages = F.lazy(() =>
      Observable.fromEvent2<LogEvent, Application.LogCallback>(
        onEvent => {
          Application.logMessageReceivedThreaded += callback;
          return callback;
          
          void callback(string message, string backtrace, LogType type) {
            // Ignore messages that we ourselves sent to Unity.
            if (message.StartsWithFast(MESSAGE_PREFIX)) return;
            var logEventTry = convertUnityMessageToLogEvent(
              message, backtrace, type, stackFramesToSkipWhenGenerating: 1 /* This stack frame */
            );
            var logEvent = logEventTry.isSuccess
              ? logEventTry.__unsafeGet
              : new LogEvent(
                LogLevel.ERROR, 
                LogEntry.fromException(
                  $"Error while converting Unity log message (type: {type}): {message}, backtrace: [{backtrace}]", 
                  logEventTry.__unsafeException
                ));

            ASync.OnMainThread(
              () => onEvent(logEvent),
              // Do not run code that might throw an exception itself in logMessageReceived.
              runNowIfOnMainThread: false
            );
          }
        },
        callback => Application.logMessageReceivedThreaded -= callback
      )
    );

    static LogLevel convertLevel(LogType type) {
      switch (type) {
        case LogType.Error:
        case LogType.Exception:
        case LogType.Assert:
          return LogLevel.ERROR;
        case LogType.Warning:
          return LogLevel.WARN;
        case LogType.Log:
          return LogLevel.INFO;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }
    }
  }
}