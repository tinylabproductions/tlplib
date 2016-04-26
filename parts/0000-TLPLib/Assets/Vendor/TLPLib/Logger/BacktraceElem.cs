using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Logger {
  public struct BacktraceElem {
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

      static readonly IEqualityComparer<FileInfo> FileLineNoComparerInstance = new FileLineNoEqualityComparer();
      public static IEqualityComparer<FileInfo> fileLineNoComparer { get { return FileLineNoComparerInstance; } }

      #endregion

      public override string ToString() { return file + ":" + lineNo; }
    }

    public readonly string method;
    public readonly Option<FileInfo> fileInfo;

    public BacktraceElem(string method, Option<FileInfo> fileInfo) {
      this.method = method;
      this.fileInfo = fileInfo;
    }

    /* Is this trace frame is in our application code? */
    public bool inApp { get { return !method.StartsWith("UnityEngine.") && !method.StartsWith("com.tinylabproductions.TLPLib.Logger."); } }

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
      // backtrace may be empty in release mode.
      if (string.IsNullOrEmpty(backtrace)) {
        // TODO: we can optimize this to make less garbage
        var trace = new StackTrace(0, true);
        var frames = trace.GetFrames();
        if (frames == null) return new List<BacktraceElem>();
        return frames.Select(frame => {
          var method = methodStringFromFrame(frame);
          if (frame.GetFileLineNumber() == 0)
            return new BacktraceElem(method, F.none<FileInfo>());
          else
            return new BacktraceElem(method, F.some(new FileInfo(frame.GetFileName(), frame.GetFileLineNumber())));
        }).Where(bt => bt.inApp).ToList();
      }
      return Regex.Split(backtrace, "\n")
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s))
        // ReSharper disable once ConvertClosureToMethodGroup
        .Select(s => parseUnityBacktraceLine(s))
        .ToList();
    }

    // Copied from StackTrace.ToString decompiled source
    static string methodStringFromFrame(StackFrame sf) {
      var sb = new StringBuilder();
      MethodBase mb = sf.GetMethod();
      if (mb != null) {
        Type t = mb.DeclaringType;
        // if there is a type (non global method) print it
        if (t != null) {
          sb.Append(t.FullName.Replace('+', '.'));
          sb.Append(".");
        }
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
