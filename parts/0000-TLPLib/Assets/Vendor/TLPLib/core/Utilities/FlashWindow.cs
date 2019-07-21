using JetBrains.Annotations;

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
#if UNITY_STANDALONE_WIN
      new FlashWindowWin32();
#else
      new FlashWindowNoOp();
#endif
  }

  class FlashWindowNoOp : IFlashWindow {
    public bool Flash() => false;
    public bool Flash(uint count) => false;
    public bool Start() => false;
    public bool Stop() => false;
  }
}