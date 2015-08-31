using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Logger {
  public struct BacktraceElem {
    public struct FileInfo {
      public readonly string file;
      public readonly int lineNo;

      public FileInfo(string file, int lineNo) {
        this.file = file;
        this.lineNo = lineNo;
      }

      public override string ToString() { return file + ":" + lineNo; }
    }

    public readonly string method;
    public readonly Option<FileInfo> fileInfo;

    public BacktraceElem(string method, Option<FileInfo> fileInfo) {
      this.method = method;
      this.fileInfo = fileInfo;
    }

    public override string ToString() {
      return method + fileInfo.fold(string.Empty, fi => " (at " + fi.ToString() + ")");
    }

    /*
    Example backtrace:
UnityEngine.Debug:LogError(Object)
com.tinylabproductions.TLPLib.Logger.Log:error(Object) (at Assets/Vendor/TLPLib/Logger/Log.cs:14)
Assets.Code.Main:<Awake>m__32() (at Assets/Code/Main.cs:60)
com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext() (at Assets/Vendor/TLPLib/Concurrent/ASync.cs:175)
    */
    public static readonly Regex UNITY_BACKTRACE_LINE = new Regex(@"^(.+?)( \(at (.*?):(\d+)\))?$");

    public static List<BacktraceElem> parseUnityBacktrace(string backtrace) {
      return Regex.Split(backtrace, "\n")
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        // ReSharper disable once ConvertClosureToMethodGroup
        .Select(s => parseUnityBacktraceLine(s))
        .ToList();
    }

    public static BacktraceElem parseUnityBacktraceLine(string line) {
      var match = UNITY_BACKTRACE_LINE.Match(line);

      var method = match.Groups[1].Value;
      var hasLineNo = match.Groups[2].Success;
      if (hasLineNo) {
        var file = match.Groups[3].Value;
        var lineNo = int.Parse(match.Groups[4].Value);
        return new BacktraceElem(method, F.some(new FileInfo(file, lineNo)));
      }
      else {
        return new BacktraceElem(method, F.none<FileInfo>());
      }
    }
  }
}
