using System;
using System.Collections.Generic;
using System.IO;
using com.tinylabproductions.TLPLib.Filesystem;

namespace com.tinylabproductions.TLPLib.Data.scenes {
  public struct ScenePath : IEquatable<ScenePath>, IComparable<ScenePath> {
    public readonly string path;

    #region Equality

    public bool Equals(ScenePath other) => string.Equals(path, other.path);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ScenePath && Equals((ScenePath)obj);
    }

    public override int GetHashCode() => (path != null ? path.GetHashCode() : 0);

    public static bool operator ==(ScenePath left, ScenePath right) => left.Equals(right);
    public static bool operator !=(ScenePath left, ScenePath right) => !left.Equals(right);

    #endregion

    #region Comparer

    public int CompareTo(ScenePath other) {
      return string.Compare(path, other.path, StringComparison.Ordinal);
    }

    sealed class PathEqualityComparer : IEqualityComparer<ScenePath> {
      public bool Equals(ScenePath x, ScenePath y) {
        return string.Equals(x.path, y.path);
      }

      public int GetHashCode(ScenePath obj) {
        return (obj.path != null ? obj.path.GetHashCode() : 0);
      }
    }

    public static IEqualityComparer<ScenePath> pathComparer { get; } = new PathEqualityComparer();

    #endregion

    public ScenePath(string path) { this.path = path; }

    public override string ToString() => $"{nameof(ScenePath)}({path})";

    public SceneName toSceneName => new SceneName(Path.GetFileNameWithoutExtension(path));

    public PathStr toPathStr => PathStr.a(path);

    public static implicit operator string(ScenePath s) => s.path;
    public static implicit operator SceneName(ScenePath s) => s.toSceneName;
  }
}