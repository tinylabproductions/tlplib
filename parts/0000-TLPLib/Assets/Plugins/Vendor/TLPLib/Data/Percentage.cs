using System;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Percentage : IEquatable<Percentage> {
    // [0, 1]
    public readonly float value;

    public Percentage(float value) {
      this.value = value;
    }

    public bool Equals(Percentage other) {
      return value.Equals(other.value);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Percentage && Equals((Percentage) obj);
    }

    public override int GetHashCode() {
      return value.GetHashCode();
    }

    public static bool operator ==(Percentage left, Percentage right) {
      return left.Equals(right);
    }

    public static bool operator !=(Percentage left, Percentage right) {
      return !left.Equals(right);
    }
  }
}