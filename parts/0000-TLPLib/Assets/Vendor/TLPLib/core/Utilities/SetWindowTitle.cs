// UNITY_STANDALONE_XXX while in editor tell us what target platform is set in the build settings.
// We want to know what platform our code is running on, so different checks need to be made for build and editor.
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
#define WINDOWS_RUNTIME
#endif

#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
#define OSX_RUNTIME
#endif

using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.core.Utilities {
  [PublicAPI] public interface ISetWindowTitle {
    /// <summary>Sets the current operating system window title.</summary>
    /// <returns>true if successful, false otherwise</returns>
    bool setWindowTitle(string title);
  }
  
  [PublicAPI] public class SetWindowTitle {
    /// <summary>
    /// Check the runtime platform, not the build target, to prevent Unity running on Mac but targeting Windows from
    /// trying to use Win32 APIs.
    /// <see cref="win32_api.SetWindowTitle"/> is only defined in WINDOWS_RUNTIME, same goes for osx version, so
    /// additional checks need to be made.
    /// </summary>
    public static readonly ISetWindowTitle instance =
      Application.platform switch {
#if WINDOWS_RUNTIME
        RuntimePlatform.WindowsPlayer => new win32_api.SetWindowTitle(),
        // When running in batch mode we do not have a window to set window title on.
        // You would think that setting the window title on Unity would ruin it forever but turns out that it does not
        // and Unity restores the window title when exiting play mode
        RuntimePlatform.WindowsEditor when !Application.isBatchMode => new win32_api.SetWindowTitle(),
#endif
#if OSX_RUNTIME
        RuntimePlatform.OSXPlayer => new osx.SetWindowTitle(),
        // This works the same way as windows editor
        RuntimePlatform.OSXEditor when !Application.isBatchMode => new osx.SetWindowTitle(),
#endif
        _ => new NoOpSetWindowTitle()
      };
  }

  class NoOpSetWindowTitle : ISetWindowTitle {
    public bool setWindowTitle(string title) => false;
  }
}