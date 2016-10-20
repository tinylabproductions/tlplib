using System;

namespace com.tinylabproductions.TLPLib.Functional {
  public struct Unit : IEquatable<Unit> {
    public static Unit instance { get; } = new Unit();
    public override string ToString() => "()";

    #region Equality

    public bool Equals(Unit other) => true;

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Unit && Equals((Unit) obj);
    }

    public override int GetHashCode() => nameof(Unit).GetHashCode();

    public static bool operator ==(Unit left, Unit right) { return left.Equals(right); }
    public static bool operator !=(Unit left, Unit right) { return !left.Equals(right); }

    #endregion
  }
}
