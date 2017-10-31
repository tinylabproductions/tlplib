using System;

namespace com.tinylabproductions.TLPLib.Data.scenes {
  public struct ScenePath : IEquatable<ScenePath> {
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

    public ScenePath(string path) { this.path = path; }

    public override string ToString() => $"{nameof(ScenePath)}({path})";

    public static implicit operator string(ScenePath s) => s.path;
  }
}