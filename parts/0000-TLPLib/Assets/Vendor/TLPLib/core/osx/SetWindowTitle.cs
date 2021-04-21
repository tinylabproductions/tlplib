#if UNITY_EDITOR_OSX || (UNITY_STANDALONE_OSX && !UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;
using com.tinylabproductions.TLPLib.core.Utilities;
using pzd.lib.exts;

namespace com.tinylabproductions.TLPLib.osx {
  public class SetWindowTitle : ISetWindowTitle {
    public bool setWindowTitle(string title) {
      tlplibOSXWindowSetTitle(title);
      return true;
    }

    [DllImport("tlplib_osx")]
    static extern void tlplibOSXWindowSetTitle(string title);
  }
}
#endif