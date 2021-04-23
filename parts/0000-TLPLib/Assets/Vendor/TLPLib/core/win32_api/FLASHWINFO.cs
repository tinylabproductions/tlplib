// See Utilities.SetWindowTitle.cs for explanation.
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
using System;
using System.Runtime.InteropServices;

namespace com.tinylabproductions.TLPLib.win32_api {
  [StructLayout(LayoutKind.Sequential)] public struct FLASHWINFO {
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
}
#endif