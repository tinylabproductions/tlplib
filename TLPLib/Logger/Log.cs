#if ! DEBUG
#define MULTITHREADED
using System.Threading;
using com.tinylabproductions.TLPLib.Extensions;
using System.Collections.Generic;
#endif

using System;
using System.Diagnostics;
using System.IO;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.tinylabproductions.TLPLib.Logger {
  /**
   * Unity logging is dog slow, so we do our own logging to a file.
   * 
   * We also intercept unity logs here and add them to our own.
   **/
  public static class Log {
    public const string P_TRACE = "[TRACE]> ";
    public const string P_DEBUG = "[DEBUG]> ";
    public const string P_INFO = "[INFO]> ";
    public const string P_WARN = "[WARN]> ";
    public const string P_EXCEPTION = "[ERROR:EXCEPTION]> ";
    public const string P_ERROR = "[ERROR]> ";

    [Conditional("TRACE")]
    public static void trace(object o) { file(P_TRACE, o); }
    [Conditional("DEBUG")]
    public static void debug(object o) { file(P_DEBUG, o); }
    public static void info(object o) { file(P_INFO, o); }
    public static void warn(object o) { file(P_WARN, o); }
    public static void error(Exception ex) { Debug.LogException(ex); }
    public static void error(object o) { Debug.LogError(o); }

    public static void file(string prefix, object o) { FileLog.log(prefix, o); }

    public static string debugObj<A>(this A obj) { return obj + "(" + obj.GetHashCode() + ")"; }

    public static string fileName {
      get { return ((FileStream) FileLog.logfile.BaseStream).Name; }
    }
  }

  class FileLog {
#if MULTITHREADED
    private static readonly LinkedList<Tpl<string, DateTime, object>> messages = 
      new LinkedList<Tpl<string, DateTime, object>>();
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
      Application.RegisterLogCallbackThreaded(unityLogs);
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
          Thread.Sleep(0);
        }
      }).Start();
#else
      Application.RegisterLogCallback(unityLogs);
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
      return string.Format("{0}:{1}:{2}.{3}", t.Hour, t.Minute, t.Second, t.Millisecond);
    }

    private static void unityLogs(string message, string stackTrace, LogType type) {
      string prefix = null;
      switch (type) {
        case LogType.Error:
        case LogType.Assert:
          prefix = Log.P_ERROR;
          break;
        case LogType.Exception:
          prefix = Log.P_EXCEPTION;
          break;
        case LogType.Warning:
          prefix = Log.P_WARN;
          break;
        case LogType.Log:
          prefix = Log.P_INFO;
          break;
      }
      log(prefix, String.IsNullOrEmpty(stackTrace) ? message : message + "\n" + stackTrace);
    }

    public static void log(string prefix, object o) {
      var t = F.t(prefix, DateTime.Now, o);
#if MULTITHREADED
      lock(messages) messages.AddLast(t);
#else
      write(t);
#endif
    }
  }
}
