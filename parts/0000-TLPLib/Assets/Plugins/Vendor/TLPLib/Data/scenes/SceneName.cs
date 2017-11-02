using System;

namespace com.tinylabproductions.TLPLib.Data.scenes {
  public struct SceneName : IEquatable<SceneName> {
    public readonly string name;

    #region Equality

    public bool Equals(SceneName other) => string.Equals(name, other.name);

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is SceneName && Equals((SceneName)obj);
    }

    public override int GetHashCode() => (name != null ? name.GetHashCode() : 0);

    public static bool operator ==(SceneName left, SceneName right) => left.Equals(right);
    public static bool operator !=(SceneName left, SceneName right) => !left.Equals(right);

    #endregion

    public SceneName(string name) { this.name = name; }

    public override string ToString() => $"{nameof(SceneName)}({name})";

    public static implicit operator string(SceneName s) => s.name;
  }
}