using System.Text.RegularExpressions;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.log;

namespace com.tinylabproductions.TLPLib.Logger {
  public static class BacktraceElemUnity {
    /*
    Example backtrace:
UnityEngine.Debug:LogError(Object)
com.tinylabproductions.TLPLib.Logger.Log:error(Object) (at Assets/Vendor/TLPLib/Logger/Log.cs:14)
Assets.Code.Main:<Awake>m__32() (at Assets/Code/Main.cs:60)
com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext() (at Assets/Vendor/TLPLib/Concurrent/ASync.cs:175)
    */
    public static readonly Regex UNITY_BACKTRACE_LINE = new Regex(@"^(.+?)( \(at (.*?):(\d+)\))?$");

    public static BacktraceElem parseBacktraceLine(string line) {
      var match = UNITY_BACKTRACE_LINE.Match(line);

      var method = match.Groups[1].Value;
      var hasLineNo = match.Groups[2].Success;
      return new BacktraceElem(
        method,
        hasLineNo
          ? Some.a(new BacktraceElem.FileInfo(match.Groups[3].Value, int.Parse(match.Groups[4].Value)))
          : Option<BacktraceElem.FileInfo>.None
      );
    }
  }

  public static class BacktraceElemExts {
    /// <summary>Is this trace frame is in our application code?</summary>
    public static bool inApp(this BacktraceElem elem) => !elem.method.StartsWithFast(nameof(UnityEngine) + ".");
  }
}
