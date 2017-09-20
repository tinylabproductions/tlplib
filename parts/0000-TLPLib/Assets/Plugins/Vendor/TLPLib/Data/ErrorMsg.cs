using System;

namespace com.tinylabproductions.TLPLib.Data {
  public struct ErrorMsg : IEquatable<ErrorMsg> {
    public readonly string s;

    public ErrorMsg(string s) { this.s = s; }

    #region Equality

    public bool Equals(ErrorMsg other) {
      return string.Equals(s, other.s);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ErrorMsg && Equals((ErrorMsg) obj);
    }

    public override int GetHashCode() {
      return (s != null ? s.GetHashCode() : 0);
    }

    public static bool operator ==(ErrorMsg left, ErrorMsg right) { return left.Equals(right); }
    public static bool operator !=(ErrorMsg left, ErrorMsg right) { return !left.Equals(right); }

    #endregion

    public override string ToString() => $"{nameof(ErrorMsg)}({s})";
  }
}