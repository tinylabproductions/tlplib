#define MULTITHREADED
using System.Threading;
using com.tinylabproductions.TLPLib.Extensions;
using System.Collections.Generic;

using System;
using System.Diagnostics;
using System.IO;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.tinylabproductions.TLPLib.Logger {
  public class LogI {
    public const string P_TRACE = "[TRACE]> ";
    public const string P_DEBUG = "[DEBUG]> ";
    public const string P_INFO = "[INFO]> ";
    public const string P_WARN = "[WARN]> ";
    public const string P_EXCEPTION = "[ERROR:EXCEPTION]> ";
    public const string P_ERROR = "[ERROR]> ";

    public readonly string scope;

    public LogI() { scope = ""; }
    public LogI(string scope) { this.scope = scope; }

    [Conditional("TRACE")] public void trace(object o) { file(P_TRACE, o); }
    [Conditional("DEBUG")] public void debug(object o) { file(P_DEBUG, o); }
    public void info(object o) { file(P_INFO, o); }
    public void warn(object o) { file(P_WARN, o); }
    public void error(Exception ex) { Debug.LogException(ex); }
    public void error(object o) { Debug.LogError(o); }

    [Conditional("DEBUG")]
    public void inDebug(Act a) { a(); }

    public void stacktrace() { info(Environment.StackTrace); }
    public LogI scoped(string addedScope) { return new LogI(
      scope == "" ? addedScope : scope + ">" + addedScope
    ); }

    void file(string prefix, object o) { FileLog.log(scopedPrefix(prefix), o); }

    string scopedPrefix(string prefix) { return scope == "" ? prefix : prefix + "[" + scope + "] "; }
  }

  /**
   * Unity logging is dog slow, so we do our own logging to a file.
   * 
   * We also intercept unity logs here and add them to our own.
   **/
  public static class Log {
    public readonly static LogI logger = new LogI();

    [Conditional("TRACE")]
    public static void trace(object o) { logger.trace(o); }
    [Conditional("DEBUG")]
    public static void debug(object o) { logger.debug(o); }
    public static void info(object o) { logger.info(o); }
    public static void warn(object o) { logger.warn(o); }
    public static void error(Exception ex) { logger.error(ex); }
    public static void error(object o) { logger.error(o); }

    [Conditional("DEBUG")]
    public static void inDebug(Act a) { logger.inDebug(a); }
    public static void stacktrace() { logger.stacktrace(); }

    public static string debugObj<A>(this A obj) { return obj + "(" + obj.GetHashCode() + ")"; }

    public static string fileName {
      get { return ((FileStream) FileLog.logfile.BaseStream).Name; }
    }
  }

  class FileLog {
#if MULTITHREADED
    static readonly LinkedList<Tpl<string, DateTime, object>> messages = 
      new LinkedList<Tpl<string, DateTime, object>>();
    static readonly AutoResetEvent hasMessagesEvt = new AutoResetEvent(false);
#endif

    public readonly static StreamWriter logfile;

    static FileLog() {
      var t = tryOpen(Application.temporaryCachePath + "/runtime.log");
      logfile = t._1;
      var logfilePath = t._2;

      ASync.onAppQuit.subscribe(_ => {
        log("\n\n", "############ Log closed ############\n\n");
        logfile.Close();
      });

      Debug.Log("Runtime Logfile: " + logfilePath);
      log("\n\n", "############ Log opened ############\n\n");

#if MULTITHREADED
      // Seems to need both.
      Application.logMessageReceived += unityLogs;
      Application.logMessageReceivedThreaded += unityLogs;
      new Thread(() => {
        while (true) {
          var tOpt = F.none<Tpl<string, DateTime, object>>();

          lock (messages) {
            if (! messages.isEmpty()) {
              tOpt = F.some(messages.First.Value);
              messages.RemoveFirst();
            }
          }

          tOpt.each(write);
          hasMessagesEvt.WaitOne();
        }
      }).Start();
#else
      Application.logMessageReceived += unityLogs;
#endif
    }

    private static StreamWriter open(string path) {
      return new StreamWriter(File.Open(
        path,
        Debug.isDebugBuild ? FileMode.Append : FileMode.Create,
        FileAccess.Write, FileShare.Read
      )) { AutoFlush = true };
    }

    private static Tpl<StreamWriter, string> tryOpen(string path) {
      var i = 0;
      while (true) {
        var realPath = i == 0 ? path : path + "." + i;
        try { return F.t(open(realPath), realPath); }
        catch (IOException) {
          if (File.Exists(realPath)) i++;
          else throw;
        }
      }
    }

    private static void write(Tpl<string, DateTime, object> t) {
      lock (logfile) { logfile.WriteLine(dt(t._2) + "|" + t._1 + t._3); }
    }

    private static string dt(DateTime t) {
      return $"{t.Hour}:{t.Minute}:{t.Second}.{t.Millisecond}";
    }

    private static void unityLogs(string message, string stackTrace, LogType type) {
      string prefix = null;
      switch (type) {
        case LogType.Error:
        case LogType.Assert:
          prefix = LogI.P_ERROR;
          break;
        case LogType.Exception:
          prefix = LogI.P_EXCEPTION;
          break;
        case LogType.Warning:
          prefix = LogI.P_WARN;
          break;
        case LogType.Log:
          prefix = LogI.P_INFO;
          break;
      }
      log(prefix, string.IsNullOrEmpty(stackTrace) ? message : message + "\n" + stackTrace);
    }

    public static void log(string prefix, object o) {
      var t = F.t(prefix, DateTime.Now, o);
#if MULTITHREADED
      lock(messages) messages.AddLast(t);
      hasMessagesEvt.Set();
#else
      write(t);
#endif
    }
  }
}
