using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Configuration;

namespace com.tinylabproductions.TLPLib.ABTest {
  public struct ABTestVariant : IEquatable<ABTestVariant> {
    public readonly String name;
    public readonly int chanceWeight;

    #region Equality

    public bool Equals(ABTestVariant other) {
      return string.Equals(name, other.name) && chanceWeight == other.chanceWeight;
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ABTestVariant && Equals((ABTestVariant) obj);
    }

    public override int GetHashCode() {
      unchecked { return ((name != null ? name.GetHashCode() : 0) * 397) ^ chanceWeight; }
    }

    public static bool operator ==(ABTestVariant left, ABTestVariant right) { return left.Equals(right); }
    public static bool operator !=(ABTestVariant left, ABTestVariant right) { return !left.Equals(right); }

    sealed class NameChanceWeightEqualityComparer : IEqualityComparer<ABTestVariant> {
      public bool Equals(ABTestVariant x, ABTestVariant y) {
        return string.Equals(x.name, y.name) && x.chanceWeight == y.chanceWeight;
      }

      public int GetHashCode(ABTestVariant obj) {
        unchecked { return ((obj.name != null ? obj.name.GetHashCode() : 0) * 397) ^ obj.chanceWeight; }
      }
    }

    static readonly IEqualityComparer<ABTestVariant> NameChanceWeightComparerInstance = new NameChanceWeightEqualityComparer();

    public static IEqualityComparer<ABTestVariant> nameChanceWeightComparer {
      get { return NameChanceWeightComparerInstance; }
    }

    #endregion

    public ABTestVariant(string name, int chanceWeight) {
      this.name = name;
      this.chanceWeight = chanceWeight;
    }

    public static ABTestVariant fromConfig(IConfig cfg) {
      return new ABTestVariant(cfg.getString("name"), cfg.getInt("chance_weigth"));
    }

    public override string ToString() { return string.Format(
      "ABTestVariant[name: {0}, chanceWeight: {1}]",
      name, chanceWeight
    ); }
  }
}
