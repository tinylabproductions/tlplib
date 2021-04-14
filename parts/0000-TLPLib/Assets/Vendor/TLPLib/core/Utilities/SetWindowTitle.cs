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
    /// trying to use Win32 APIs. However because <see cref="win32_api.FlashWindowWin32"/> is only defined in
    /// UNITY_STANDALONE_WIN, we also need to check the build target.
    /// </summary>
    public static readonly ISetWindowTitle instance =
      Application.platform switch {
#if UNITY_STANDALONE_WIN
        RuntimePlatform.WindowsPlayer => new win32_api.SetWindowTitle(),
        // When running in batch mode we do not have a window to set window title on.
        // You would think that setting the window title on Unity would ruin it forever but turns out that it does not
        // and Unity restores the window title when exiting play mode
        RuntimePlatform.WindowsEditor when !Application.isBatchMode => new win32_api.SetWindowTitle(),
#endif
        _ => new NoOpSetWindowTitle()
      };
  }

  class NoOpSetWindowTitle : ISetWindowTitle {
    public bool setWindowTitle(string title) => false;
  }
}