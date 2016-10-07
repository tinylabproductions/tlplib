using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  class DebugUtilsLogException : Exception {
    public readonly object o;

    public DebugUtilsLogException(object o) : base(o.ToString()) { this.o = o; }
  }

  public static class DebugUtils {
    public static void log(LogType type, object o) {
      switch (type) {
        case LogType.Assert:
          Debug.Assert(false, o);
          break;
        case LogType.Error:
          Debug.LogError(o);
          break;
        case LogType.Exception:
          Debug.LogException(new DebugUtilsLogException(o));
          break;
        case LogType.Warning:
          Debug.LogWarning(o);
          break;
        case LogType.Log:
          Debug.Log(o);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }
    }
  }
}