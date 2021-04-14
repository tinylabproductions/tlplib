#if UNITY_STANDALONE_WIN
using System;
using System.Runtime.InteropServices;
using com.tinylabproductions.TLPLib.core.Utilities;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.win32_api {
  public class SetWindowTitle : ISetWindowTitle {
    public bool setWindowTitle(string title) {
      if (!WindowHandle.handle.valueOut(out var handle)) return false;
      // This is done because of unicode symbols
      // https://stackoverflow.com/questions/60017625/russian-characters-in-c-sharp-setwindowtextw
      //
      // https://docs.microsoft.com/en-us/windows/win32/winmsg/wm-settext
      const uint WM_SET_TEXT = 0X000C;
    
      var stringPtr = Marshal.StringToHGlobalUni(title);
      try {
        Win32API.SendMessage(handle, WM_SET_TEXT, wParam: IntPtr.Zero, lParam: stringPtr);
      }
      finally {
        Marshal.FreeHGlobal(stringPtr);
      }

      return true;
    }
  }
}
#endif