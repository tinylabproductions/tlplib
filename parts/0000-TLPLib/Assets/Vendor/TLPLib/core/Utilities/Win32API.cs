#if UNITY_STANDALONE_WIN
using System.Text;
using System;
using System.Runtime.InteropServices;
using com.tinylabproductions.TLPLib.Logger;
using static com.tinylabproductions.TLPLib.Utilities.Win32API;

namespace com.tinylabproductions.TLPLib.Utilities {
  public static class Win32API {
    /// Only returns the window if it is currently focused. Otherwise returns IntPtr.zero.
    [DllImport("User32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
    
    [StructLayout(LayoutKind.Sequential)] public class FLASHWINFO {
      /// The size of the structure in bytes.
      public uint cbSize;

      /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
      public IntPtr hwnd;

      /// The Flash Status.
      public uint dwFlags;

      /// The number of times to Flash the window.
      public uint uCount;

      /// The rate at which the Window is to be flashed, in milliseconds.
      /// If Zero, the function uses the default cursor blink rate.
      public uint dwTimeout;
    }
    
    // https://gist.github.com/mattbenic/908483ad0bedbc62ab17
    public static class WindowHandle {
      #region DLL Imports
      const string UnityWindowClassName = "UnityWndClass";

      [DllImport("kernel32.dll")]
      static extern uint GetCurrentThreadId();

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

      public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
      #endregion

      public static readonly IntPtr handle;

      static WindowHandle() {
        if (Log.d.isDebug()) Log.d.debug("Finding WIN32 window handle...");
        var h = GetActiveWindow();
        if (h == IntPtr.Zero) {
          var threadId = GetCurrentThreadId();

          EnumThreadWindows(threadId, (hWnd, lParam) => {
            var classTextB = new StringBuilder(1000);
            GetClassName(hWnd, classTextB, classTextB.Capacity);
            var classText = classTextB.ToString();
            if (Log.d.isDebug()) Log.d.debug($"Found WIN32 window {classText} at {hWnd}");
            if (classText == UnityWindowClassName) {
              h = hWnd;
              return false;
            }

            return true;
          }, IntPtr.Zero);
        }
        else {
          if (Log.d.isDebug()) Log.d.debug("WIN32 window handle found from active window.");
        }

        if (Log.d.isDebug()) Log.d.debug($"WIN32 window handle: {h}"); 
        handle = h;
      }
    }
  }
  

  class FlashWindowWin32 : IFlashWindow {
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
        var fi = Create_FLASHWINFO(
          WindowHandle.handle, FLASHW_ALL | FLASHW_TIMERNOFG, uint.MaxValue, 0
        );
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        Log.d.error(nameof(Flash), e);
      }
      return false;
    }
    
    public bool Flash(uint count) {
      try {
        var fi = Create_FLASHWINFO(WindowHandle.handle, FLASHW_ALL, count, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        Log.d.error($"{nameof(Flash)}({count})", e);
      }

      return false;
    }

    public bool Start() {
      try {
        var fi = Create_FLASHWINFO(WindowHandle.handle, FLASHW_ALL, uint.MaxValue, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        Log.d.error(nameof(Start), e);
      }

      return false;
    }

    public bool Stop() {
      try {
        var fi = Create_FLASHWINFO(WindowHandle.handle, FLASHW_STOP, uint.MaxValue, 0);
        return FlashWindowEx(ref fi);
      }
      catch (Exception e) {
        Log.d.error(nameof(Stop), e);
      }

      return false;
    }
  }
}
#endif