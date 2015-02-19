using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.tinylabproductions.TLPLib.Logger {
  public static class Log {
    [Conditional("UNITY_EDITOR"), Conditional("LOG_DEBUG")]
    public static void debug(object o) { Debug.Log("[DEBUG]> " + o); }
    public static void info(object o) { Debug.LogWarning("[INFO]> " + o); }
    public static void warn(object o) { Debug.LogWarning("[WARN]> " + o); }
    public static void error(Exception ex) { Debug.LogException(ex); }
    public static void error(object o) { Debug.LogError("[ERROR]> " + o); }
    public static void error(object o, Exception ex) { error(o + ": " + ex); error(ex); }
    [Conditional("UNITY_EDITOR")]
    public static void editor(object o) { EditorLog.log(o); }
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
