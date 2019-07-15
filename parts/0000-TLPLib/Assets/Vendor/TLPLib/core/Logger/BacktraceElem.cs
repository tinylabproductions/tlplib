using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Logger {
  public struct BacktraceElem : IEquatable<BacktraceElem>, IStr {
    public struct FileInfo : IEquatable<FileInfo> {
      public readonly string file;
      public readonly int lineNo;

      public FileInfo(string file, int lineNo) {
        this.file = file;
        this.lineNo = lineNo;
      }

      #region Equality

      public bool Equals(FileInfo other) {
        return string.Equals(file, other.file) && lineNo == other.lineNo;
      }

      public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        return obj is FileInfo && Equals((FileInfo) obj);
      }

      public override int GetHashCode() {
        unchecked { return ((file != null ? file.GetHashCode() : 0) * 397) ^ lineNo; }
      }

      public static bool operator ==(FileInfo left, FileInfo right) { return left.Equals(right); }
      public static bool operator !=(FileInfo left, FileInfo right) { return !left.Equals(right); }

      sealed class FileLineNoEqualityComparer : IEqualityComparer<FileInfo> {
        public bool Equals(FileInfo x, FileInfo y) {
          return string.Equals(x.file, y.file) && x.lineNo == y.lineNo;
        }

        public int GetHashCode(FileInfo obj) {
          unchecked { return ((obj.file != null ? obj.file.GetHashCode() : 0) * 397) ^ obj.lineNo; }
        }
      }

      public static IEqualityComparer<FileInfo> fileLineNoComparer { get; } = new FileLineNoEqualityComparer();

      #endregion

      public override string ToString() => $"{file}:{lineNo}";
    }

    public readonly string method;
    public readonly Option<FileInfo> fileInfo;

    public BacktraceElem(string method, Option<FileInfo> fileInfo) {
      this.method = method;
      this.fileInfo = fileInfo;
    }

    #region Equality

    public bool Equals(BacktraceElem other) {
      return String.Equals(method, other.method) && fileInfo.Equals(other.fileInfo);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is BacktraceElem && Equals((BacktraceElem) obj);
    }

    public override int GetHashCode() {
      unchecked { return ((method != null ? method.GetHashCode() : 0) * 397) ^ fileInfo.GetHashCode(); }
    }

    public static bool operator ==(BacktraceElem left, BacktraceElem right) { return left.Equals(right); }
    public static bool operator !=(BacktraceElem left, BacktraceElem right) { return !left.Equals(right); }

    #endregion

    /* Is this trace frame is in our application code? */
    public bool inApp => !method.StartsWithFast(nameof(UnityEngine) + ".");

    public string asString() => $"{method}{fileInfo.fold("", fi => $" (at {fi})")}";
    public override string ToString() => asString();

    /*
    Example backtrace:
UnityEngine.Debug:LogError(Object)
com.tinylabproductions.TLPLib.Logger.Log:error(Object) (at Assets/Vendor/TLPLib/Logger/Log.cs:14)
Assets.Code.Main:<Awake>m__32() (at Assets/Code/Main.cs:60)
com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext() (at Assets/Vendor/TLPLib/Concurrent/ASync.cs:175)
    */
    public static readonly Regex UNITY_BACKTRACE_LINE = new Regex(@"^(.+?)( \(at (.*?):(\d+)\))?$");

    public static BacktraceElem parseUnityBacktraceLine(string line) {
      var match = UNITY_BACKTRACE_LINE.Match(line);

      var method = match.Groups[1].Value;
      var hasLineNo = match.Groups[2].Success;
      return new BacktraceElem(
        method,
        hasLineNo
          ? F.some(new FileInfo(match.Groups[3].Value, int.Parse(match.Groups[4].Value)))
          : F.none<FileInfo>()
      );
    }
  }
}
