using System;
using System.Diagnostics;
using System.IO;
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
    public enum Level { ERROR, WARN, INFO, DEBUG }

    public static Level level = 
      Application.isEditor || Debug.isDebugBuild ? Level.DEBUG : Level.INFO;

    /* Compile time version of debug. */
    [Conditional("UNITY_EDITOR"), Conditional("LOG_DEBUG")]
    public static void cdebug(object o) { rdebug(o); }

    /* Runtime version of debug. */
    public static void rdebug(object o) { if (isDebug) Debug.Log("[DEBUG]> " + o); }
    public static bool isDebug => level >= Level.DEBUG;

    public static void info(object o) { if (isInfo) Debug.Log("[INFO]> " + o); }
    public static bool isInfo => level >= Level.INFO;

    public static void warn(object o) { if (isWarn) Debug.LogWarning("[WARN]> " + o); }
    public static bool isWarn => level >= Level.WARN;

    public static void error(Exception ex) { Debug.LogException(ex); }
    public static void error(object o) { Debug.LogError("[ERROR]> " + o); }
    public static void error(object o, Exception ex)
      { error($"{o}: {ex.Message}\n{ex.StackTrace}"); }

    [Conditional("UNITY_EDITOR")]
    public static void editor(object o) { EditorLog.log(o); }

    public static Option<Level> levelFromString(string s) {
      switch (s) {
        case "debug": return F.some(Level.DEBUG);
        case "info": return F.some(Level.INFO);
        case "warn": return F.some(Level.WARN);
        case "error": return F.some(Level.ERROR);
        default: return F.none<Level>();
      }
    }
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
