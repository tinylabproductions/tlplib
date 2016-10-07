using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using com.tinylabproductions.TLPLib.Assertions;
using com.tinylabproductions.TLPLib.Configuration;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.ABTest {
  public struct ABTest : IEquatable<ABTest> {
    readonly static SHA256Managed sha = new SHA256Managed();

    public readonly string name;
    public readonly ReadOnlyCollection<ABTestVariant> variants;
    public readonly Option<DateTime> startAt, endAt;
    public readonly string seed;

    #region Equality

    public bool Equals(ABTest other) {
      return string.Equals(name, other.name) && Equals(variants, other.variants) && startAt.Equals(other.startAt) && endAt.Equals(other.endAt) && string.Equals(seed, other.seed);
    }

    public override bool Equals(object obj) {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ABTest && Equals((ABTest) obj);
    }

    public override int GetHashCode() {
      unchecked {
        var hashCode = (name != null ? name.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (variants != null ? variants.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ startAt.GetHashCode();
        hashCode = (hashCode * 397) ^ endAt.GetHashCode();
        hashCode = (hashCode * 397) ^ (seed != null ? seed.GetHashCode() : 0);
        return hashCode;
      }
    }

    public static bool operator ==(ABTest left, ABTest right) { return left.Equals(right); }
    public static bool operator !=(ABTest left, ABTest right) { return !left.Equals(right); }

    sealed class AbTestEqualityComparer : IEqualityComparer<ABTest> {
      public bool Equals(ABTest x, ABTest y) {
        return string.Equals(x.name, y.name) && Equals(x.variants, y.variants) && x.startAt.Equals(y.startAt) && x.endAt.Equals(y.endAt) && string.Equals(x.seed, y.seed);
      }

      public int GetHashCode(ABTest obj) {
        unchecked {
          var hashCode = (obj.name != null ? obj.name.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ (obj.variants != null ? obj.variants.GetHashCode() : 0);
          hashCode = (hashCode * 397) ^ obj.startAt.GetHashCode();
          hashCode = (hashCode * 397) ^ obj.endAt.GetHashCode();
          hashCode = (hashCode * 397) ^ (obj.seed != null ? obj.seed.GetHashCode() : 0);
          return hashCode;
        }
      }
    }

    static readonly IEqualityComparer<ABTest> AbTestComparerInstance = new AbTestEqualityComparer();

    public static IEqualityComparer<ABTest> abTestComparer {
      get { return AbTestComparerInstance; }
    }

    #endregion

    public ABTest(
      string name, ReadOnlyCollection<ABTestVariant> variants,
      Option<DateTime> startAt, Option<DateTime> endAt, string seed
    ) {
      variants.require(variants.Count > 0, "variants list must be non-empty!");

      this.name = name;
      this.variants = variants;
      this.startAt = startAt;
      this.endAt = endAt;
      this.seed = seed;
    }

    public static ABTest fromConfig(string name, IConfig cfg) {
      return new ABTest(
        name,
        // ReSharper disable once ConvertClosureToMethodGroup - Mono compiler bug
        cfg.getSubConfigList("variants").Select(c => ABTestVariant.fromConfig(c)).ToList().AsReadOnly(),
        cfg.optDateTime("start_at"), cfg.optDateTime("end_at"),
        cfg.optString("seed").getOrElse(name)
      );
    }

    public bool isRunningAt(DateTime time) {
      return startAt.map(t => time >= t).getOrElse(true) &&
             endAt.map(t => time <= t).getOrElse(true);
    }

    public bool isRunningNow { get { return isRunningAt(DateTime.Now); } }

    public ABTestVariant getAssignedVariant(string clientId) {
      var hashBytes = sha.ComputeHash(Encoding.ASCII.GetBytes(seed + clientId));
      var variantId = hashBytes.Sum(t => Convert.ToInt32(t)) % positiveWeightSum();

      // variantId = 0 .. N-1
      // sum = 1 .. N
      var sum = 0;
      foreach (var variant in variants) {
        sum += variant.chanceWeight;
        if (sum > variantId) return variant;
      }

      throw new IllegalStateException("Shouldn't get here");
    }

    int positiveWeightSum() {
      var sum = variants.Sum(v => v.chanceWeight);
      return sum > 0 ? sum : 1;
    }

    public override string ToString() { return string.Format(
      "ABTest[name: {0}, variants: {1}, startAt: {2}, endAt: {3}, seed: {4}]",
      name, variants.asString(false), startAt, endAt, seed
    ); }
  }
}
