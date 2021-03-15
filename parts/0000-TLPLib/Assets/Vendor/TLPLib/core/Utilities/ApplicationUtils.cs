using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.log;
using UnityEngine;
using static pzd.lib.typeclasses.Str;

namespace com.tinylabproductions.TLPLib.core.Utilities {
  [PublicAPI] public static class ApplicationUtils {
    [LazyProperty] static ILog log => Log.d.withScope(nameof(ApplicationUtils));

    /// <see cref="quit(byte)"/>
    public static void quit() => quit(0);
    
    /// <summary>
    /// As <see cref="Application.Quit(int)"/> but works in Unity Editor as well.
    ///
    /// Range for the exit code differs based on operating systems and APIs used but a byte is a safe bet that should
    /// work on all configurations. 
    /// </summary>
    public static void quit(byte exitCode) {
#if UNITY_EDITOR
      // Application.Quit() does not work in the editor so
      // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
      log.mInfo($"Simulating Application.Quit({s(exitCode)}) in Unity Editor.");
      UnityEditor.EditorApplication.isPlaying = false;
#else
      UnityEngine.Application.Quit(exitCode);
#endif
    }
  }
}