// See Utilities.SetWindowTitle.cs for explanation.
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
using System;
using System.Text;
using com.tinylabproductions.TLPLib.Logger;
using GenerationAttributes;
using pzd.lib.functional;
using pzd.lib.log;
using static com.tinylabproductions.TLPLib.win32_api.Win32API;

namespace com.tinylabproductions.TLPLib.win32_api {
  // https://gist.github.com/mattbenic/908483ad0bedbc62ab17
  public static class WindowHandle {
    [LazyProperty] static ILog log => Log.d.withScope(nameof(WindowHandle));
    
    const string UnityWindowClassName = "UnityWndClass";

    public static readonly Option<IntPtr> handle;

    static WindowHandle() {
      log.mDebug("Finding WIN32 window handle...");
      var h = GetActiveWindow();
      if (h == IntPtr.Zero) {
        var threadId = GetCurrentThreadId();

        EnumThreadWindows(threadId, (hWnd, lParam) => {
          var classTextB = new StringBuilder(1000);
          GetClassName(hWnd, classTextB, classTextB.Capacity);
          var classText = classTextB.ToString();
          log.mDebug($"Found WIN32 window {classText} at {hWnd}");
          if (classText == UnityWindowClassName) {
            h = hWnd;
            return false;
          }

          return true;
        }, IntPtr.Zero);
      }
      else {
        log.mDebug("WIN32 window handle found from active window.");
      }

      log.mDebug($"WIN32 window handle: {h}"); 
      handle = h == IntPtr.Zero ? None._ : Some.a(h);
    }
  }
}
#endif