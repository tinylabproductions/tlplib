using System;
using System.Collections.Generic;
using System.IO;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Filesystem {
  public struct PathStr : IEquatable<PathStr> {
    public readonly string path;

    public PathStr(string path) {
      this.path = path.Replace(Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
    }
    public static PathStr a(string path) { return new PathStr(path); }

    #region Equality

    public bool Equals(PathStr other) {
      return string.Equals(path, other.path);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is PathStr && Equals((PathStr) obj);
    }

    public override int GetHashCode() {
      return (path != null ? path.GetHashCode() : 0);
    }

    public static bool operator ==(PathStr left, PathStr right) { return left.Equals(right); }
    public static bool operator !=(PathStr left, PathStr right) { return !left.Equals(right); }

    sealed class PathEqualityComparer : IEqualityComparer<PathStr> {
      public bool Equals(PathStr x, PathStr y) {
        return string.Equals(x.path, y.path);
      }

      public int GetHashCode(PathStr obj) {
        return (obj.path != null ? obj.path.GetHashCode() : 0);
      }
    }

    public static IEqualityComparer<PathStr> pathComparer { get; } = new PathEqualityComparer();

    #endregion

    public static PathStr operator /(PathStr s1, string s2) {
      return new PathStr(Path.Combine(s1.path, s2));
    }

    public static implicit operator string(PathStr s) { return s.path; }

    public PathStr dirname => new PathStr(Path.GetDirectoryName(path));
    public PathStr basename => new PathStr(Path.GetFileName(path));
    public string extension => Path.GetExtension(path);

    public PathStr ensureBeginsWith(PathStr p) {
      return path.StartsWith(p.path) ? this : p / path;
    }

    public override string ToString() { return path; }
    public string unixString => ToString().Replace('\\', '/');
  }

  public static class PathStrExts {
    static Option<PathStr> onCondition(this string s, bool condition)
      { return (condition && s != null).opt(new PathStr(s)); }

    public static Option<PathStr> asFile(this string s)
      { return s.onCondition(File.Exists(s)); }

    public static Option<PathStr> asDirectory(this string s)
      { return s.onCondition(Directory.Exists(s)); }
  }
}
