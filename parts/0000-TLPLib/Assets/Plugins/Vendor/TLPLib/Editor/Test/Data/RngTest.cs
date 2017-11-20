using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  public class RngTest {
    static Rng newRng => new Rng(new Rng.Seed(100));

    static void test<A>(
      Fn<Rng, Tpl<Rng, A>> nextA, A min, A max, Option<ImmutableHashSet<A>> allOpt,
      uint iterations = 10000, double tolerance = 0.9
    ) where A : IComparable<A> {
      var rng = newRng;
      var seen = new Dictionary<A, uint>();
      for (var idx = 0u; idx < iterations; idx++) {
        var t = nextA(rng);
        rng = t._1;
        var a = t._2;
        if (a.lt(min) || a.gt(max)) Assert.Fail(
          $"rng should not generate higher than {min} and lower than {max}, but found {a}"
        );
        seen[a] = seen.getOrElse(a, 0u) + 1;
      }

      foreach (var all in allOpt) {
        var seenList = seen.Keys.ToImmutableHashSet();
        var notGenerated = all.Except(seenList);
        notGenerated.shouldBeEmpty(
          "All values should have been generated, but these ones were not."
        );
      }

      var maxOccurences = 
        seen.maxBy(Comparable.uint_, _ => _.Value).getOrThrow("empty range in test!");
      var occurenceDict = 
        seen
        .Select(kv => F.kv(kv.Key, (double) kv.Value / maxOccurences.Value))
        .ToImmutableDictionary();
      Log.d.info(occurenceDict.asDebugString());
      occurenceDict.shouldNotContain(
        kv => kv.Value < tolerance,
        kv => 
          $"All values should be generated uniformly, but {kv.Key} " +
          $"was only generated {kv.Value * 100}% as often as the most generated value " +
          $"'{maxOccurences.Key}' (which were generated {maxOccurences.Value} times)"
      );
    }

    [Test]
    public void TestIntRange() {
      const int min = -3, max = 3;
      var range = new Range(min, max);
      test(rng => rng.nextIntInRangeT(range), min, max, range.ToImmutableHashSet().some());
    }

    [Test]
    public void TestUIntRange() {
      const uint min = 3, max = 6;
      var range = new URange(min, max);
      test(rng => rng.nextUIntInRangeT(range), min, max, range.ToImmutableHashSet().some());
    }

    [Test]
    public void TestFloatRange() {
      const float min = 1f, max = 1.0001f;
      test(
        rng => rng.nextFloatInRangeT(new FRange(min, max)), min, max,
        Option<ImmutableHashSet<float>>.None, iterations: 100000,
        /**
         * Imagine float having 3 discrete values.
         * 
         *   |       |       |
         * 0 |---+---+---+---| 1
         *   |       |       |
         *   A       B       C
         * 
         * If you pick a point randomly between 0 and 1 and move towards nearest value, you
         * would get A 1/4th of the time, B 2/4th of the time and C 1/4th of the time.
         */
        tolerance: 0.25
      );
    }
  }
}