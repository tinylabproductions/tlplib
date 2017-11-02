using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

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
        return String.Equals(file, other.file) && lineNo == other.lineNo;
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
          return String.Equals(x.file, y.file) && x.lineNo == y.lineNo;
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
    public bool inApp => !method.StartsWithFast($"{nameof(UnityEngine)}.");

    public string asString() => $"{method}{fileInfo.fold("", fi => $" (at {fi})")}";
    public override string ToString() => asString();

    public static BacktraceElem parseUnityBacktraceLine(string line) {
      var match = Backtrace.UNITY_BACKTRACE_LINE.Match(line);

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

  static class StackFrameExts {
    public static BacktraceElem toBacktraceElem(this StackFrame frame) {
      // TODO: we can optimize this to make less garbage
      // we could reuse StringBuilder in StackFrameExts.methodString
      // but I am not sure if the impact would be noticeable

      var declaringClass = frame.declaringClassString();
      var method = frame.methodString();
      return new BacktraceElem(
        $"{declaringClass}:{method}",
        frame.GetFileLineNumber() == 0
          ? F.none<BacktraceElem.FileInfo>()
          : F.some(new BacktraceElem.FileInfo(frame.GetFileName(), frame.GetFileLineNumber()))
      );
    }

    public static string declaringClassString(this StackFrame sf) {
      var mb = sf.GetMethod();
      if (mb == null) return "-";
      var t = mb.DeclaringType;
      // if there is a type (non global method) print it
      if (t == null) return "-";

      return t.FullName.Replace('+', '.');
    }

    // Copied from StackTrace.ToString decompiled source
    public static string methodString(this StackFrame sf) {
      var sb = new StringBuilder();
      var mb = sf.GetMethod();
      if (mb != null) {
        sb.Append(mb.Name);

        // deal with the generic portion of the method 
        var info = mb as MethodInfo;
        if (info != null && info.IsGenericMethod) {
          var typars = info.GetGenericArguments();
          sb.Append("[");
          var k = 0;
          var fFirstTyParam = true;
          while (k < typars.Length) {
            if (fFirstTyParam == false)
              sb.Append(",");
            else
              fFirstTyParam = false;

            sb.Append(typars[k].Name);
            k++;
          }
          sb.Append("]");
        }

        // arguments printing
        sb.Append("(");
        var pi = mb.GetParameters();
        var fFirstParam = true;
        for (var j = 0; j < pi.Length; j++) {
          if (fFirstParam == false)
            sb.Append(", ");
          else
            fFirstParam = false;

          var typeName = "<UnknownType>";
          if (pi[j].ParameterType != null)
            typeName = pi[j].ParameterType.Name;
          sb.Append(typeName);
          sb.Append(' ');
          sb.Append(pi[j].Name);
        }
        sb.Append(")");
      }
      return sb.ToString();
    }
  }
}
