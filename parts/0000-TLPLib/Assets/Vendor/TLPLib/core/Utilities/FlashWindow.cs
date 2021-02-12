using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Utilities {
  [PublicAPI] public interface IFlashWindow {
    /// <summary>Flash the Window until it receives focus.</summary>
    bool Flash();
    /// <summary>Flash the window for specified amount of times.</summary>
    bool Flash(uint count);
    bool Start();
    bool Stop();
  }

  [PublicAPI] public static class FlashWindow {
    public static readonly IFlashWindow instance =
      // Check the runtime platform, not the build target, to prevent Unity running on Mac but targeting Windows from
      // trying to use Win32 APIs.
      Application.platform switch {
        RuntimePlatform.WindowsPlayer => new FlashWindowWin32(),
        // When running in batch mode we don not have a windows to flash on.
        RuntimePlatform.WindowsEditor when !Application.isBatchMode => new FlashWindowWin32(),
        _ => new FlashWindowNoOp()
      };
  }

  class FlashWindowNoOp : IFlashWindow {
    public bool Flash() => false;
    public bool Flash(uint count) => false;
    public bool Start() => false;
    public bool Stop() => false;
  }
}