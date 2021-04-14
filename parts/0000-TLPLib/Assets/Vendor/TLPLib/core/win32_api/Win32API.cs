#if UNITY_STANDALONE_WIN
using System.Text;
using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.win32_api {
  [PublicAPI] public static class Win32API {
    /// Only returns the window if it is currently focused. Otherwise returns <see cref="IntPtr.Zero"/>.
    [DllImport("User32.dll")]
    public static extern IntPtr GetActiveWindow();
    
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessage
    /// </summary>
    [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
    
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getcurrentthreadid
    /// </summary>
    [DllImport("kernel32.dll")]
    public static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    /// <summary>
    /// To continue enumeration, the callback function must return TRUE; to stop enumeration, it must return FALSE.
    /// </summary>
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumthreadwindows
    /// </summary>
    /// <param name="dwThreadId">Thread ID obtained from <see cref="GetCurrentThreadId"/></param>
    /// <param name="lpEnumFunc"></param>
    /// <param name="lParam">An application-defined value to be passed to the callback function.</param>
    /// <returns>
    /// If the callback function returns TRUE for all windows in the thread specified by dwThreadId, the return value
    /// is TRUE. If the callback function returns FALSE on any enumerated window, or if there are no windows found in
    /// the thread specified by dwThreadId, the return value is FALSE.
    /// </returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpEnumFunc, IntPtr lParam);
  }
}
#endif