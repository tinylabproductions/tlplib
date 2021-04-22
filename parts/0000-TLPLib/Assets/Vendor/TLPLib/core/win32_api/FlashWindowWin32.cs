// See Utilities.SetWindowTitle.cs for explanation
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Utilities;
using GenerationAttributes;
using pzd.lib.exts;
using pzd.lib.log;
using static com.tinylabproductions.TLPLib.win32_api.Win32API;

namespace com.tinylabproductions.TLPLib.win32_api {
  class FlashWindowWin32 : IFlashWindow {
    [LazyProperty] static ILog log => Log.d.withScope(nameof(FlashWindowWin32));
    
    /// Stop flashing. The system restores the window to its original state.
    public const uint FLASHW_STOP = 0;

    /// Flash the window caption.
    public const uint FLASHW_CAPTION = 1;

    /// Flash the taskbar button.
    public const uint FLASHW_TRAY = 2;

    /// Flash both the window caption and taskbar button.
    /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
    public const uint FLASHW_ALL = 3;

    /// Flash continuously, until the FLASHW_STOP flag is set.
    public const uint FLASHW_TIMER = 4;

    /// Flash continuously until the window comes to the foreground.
    public const uint FLASHW_TIMERNOFG = 12;

    static FLASHWINFO Create_FLASHWINFO(IntPtr handle, uint flags, uint count, uint timeout) {
      var fi = new FLASHWINFO {hwnd = handle, dwFlags = flags, uCount = count, dwTimeout = timeout};
      fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
      return fi;
    }

    public bool Flash() {
      try {
        if (!WindowHandle.handle.valueOut(out var handle)) return false;
        var fi = Create_FLASHWINFO(handle, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        log.error(nameof(Flash), e);
      }
      return false;
    }
    
    public bool Flash(uint count) {
      try {
        if (!WindowHandle.handle.valueOut(out var handle)) return false;
        var fi = Create_FLASHWINFO(handle, FLASHW_ALL, count, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        log.error($"{nameof(Flash)}({count})", e);
      }

      return false;
    }

    public bool Start() {
      try {
        if (!WindowHandle.handle.valueOut(out var handle)) return false;
        var fi = Create_FLASHWINFO(handle, FLASHW_ALL, uint.MaxValue, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        log.error(nameof(Start), e);
      }

      return false;
    }

    public bool Stop() {
      try {
        if (!WindowHandle.handle.valueOut(out var handle)) return false;
        var fi = Create_FLASHWINFO(handle, FLASHW_STOP, uint.MaxValue, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        log.error(nameof(Stop), e);
      }

      return false;
    }
  }
}
#endif