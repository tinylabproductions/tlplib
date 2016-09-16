using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Logger {
  public struct BacktraceElem : IEquatable<BacktraceElem> {
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
      return string.Equals(method, other.method) && fileInfo.Equals(other.fileInfo);
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
    public bool inApp => !method.StartsWith($"{nameof(UnityEngine)}.");

    public override string ToString() =>
      $"{method}{fileInfo.fold("", fi => $" (at {fi})")}";

    #region Parsing

    /*
    Example backtrace:
UnityEngine.Debug:LogError(Object)
com.tinylabproductions.TLPLib.Logger.Log:error(Object) (at Assets/Vendor/TLPLib/Logger/Log.cs:14)
Assets.Code.Main:<Awake>m__32() (at Assets/Code/Main.cs:60)
com.tinylabproductions.TLPLib.Concurrent.<NextFrameEnumerator>c__IteratorF:MoveNext() (at Assets/Vendor/TLPLib/Concurrent/ASync.cs:175)
    */
    public static readonly Regex UNITY_BACKTRACE_LINE = new Regex(@"^(.+?)( \(at (.*?):(\d+)\))?$");

    public static ImmutableList<BacktraceElem> parseUnityBacktrace(string backtrace) {
      return Regex.Split(backtrace, "\n")
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        // ReSharper disable once ConvertClosureToMethodGroup
        .Select(s => parseUnityBacktraceLine(s))
        .ToImmutableList();
    }

    /**
     * Creates a backtrace from the caller site.
     **/
    public static ImmutableList<BacktraceElem> generateFromHere() {
      // TODO: we can optimize this to make less garbage
      var trace = new StackTrace(0, true);
      var frames = trace.GetFrames();
      if (frames == null) return ImmutableList<BacktraceElem>.Empty;
      return frames.Select(frame => {
        var declaringClass = frame.declaringClassString();
        var method = frame.methodString();
        return new BacktraceElem(
          $"{declaringClass}:{method}",
          frame.GetFileLineNumber() == 0
          ? F.none<FileInfo>()
          : F.some(new FileInfo(frame.GetFileName(), frame.GetFileLineNumber()))
        );
      }).Where(bt => bt.inApp).ToImmutableList();
    }

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

    #endregion
  }

  static class StackFrameExts {
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
      MethodBase mb = sf.GetMethod();
      if (mb != null) {
        sb.Append(mb.Name);

        // deal with the generic portion of the method 
        if (mb is MethodInfo && ((MethodInfo) mb).IsGenericMethod) {
          Type[] typars = ((MethodInfo) mb).GetGenericArguments();
          sb.Append("[");
          int k = 0;
          bool fFirstTyParam = true;
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
        ParameterInfo[] pi = mb.GetParameters();
        bool fFirstParam = true;
        for (int j = 0; j < pi.Length; j++) {
          if (fFirstParam == false)
            sb.Append(", ");
          else
            fFirstParam = false;

          String typeName = "<UnknownType>";
          if (pi[j].ParameterType != null)
            typeName = pi[j].ParameterType.Name;
          sb.Append(typeName + " " + pi[j].Name);
        }
        sb.Append(")");
      }
      return sb.ToString();
    }
  }
}
