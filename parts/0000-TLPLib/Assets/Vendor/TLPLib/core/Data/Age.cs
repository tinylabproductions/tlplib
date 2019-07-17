using System;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;
using pzd.lib.typeclasses;

namespace com.tinylabproductions.TLPLib.Data {
  public struct Age : IEquatable<Age>, IStr {
    public readonly uint value;

    public Age(uint value) { this.value = value; }

    public override string ToString() => $"{nameof(Age)}({value})";
    public string asString() => Str.s(value);

    #region Equality

    public bool Equals(Age other) {
      return value == other.value;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Age && Equals((Age) obj);
    }

    public override int GetHashCode() {
      return (int) value;
    }

    public static bool operator ==(Age left, Age right) { return left.Equals(right); }
    public static bool operator !=(Age left, Age right) { return !left.Equals(right); }

    #endregion

    public static implicit operator uint(Age age) => age.value;

    // http://stackoverflow.com/questions/9/how-do-i-calculate-someones-age-in-c
    public static Either<string, Age> calculate(DateTime birthDate, DateTime now) {
      var age = now.Year - birthDate.Year;
      if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
        age--;
      return age >= 0
        ? Either<string, Age>.Right(new Age((uint) age))
        : $"Player age is negative: {age}";
    }
  }
}