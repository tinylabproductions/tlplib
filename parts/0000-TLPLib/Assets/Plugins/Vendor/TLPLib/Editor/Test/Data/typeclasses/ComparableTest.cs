using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public class ComparableTest {
    static readonly ImmutableList<int> seq = ImmutableList.Create(1, 6, 2, 5, 2, 7, 2, 19, -1, -5);
    static readonly Comparable<int> cmp = Comparable.integer;

    [Test]
    public void TestMin() {
      seq.min(cmp).shouldBeSome(seq.Min());
      ImmutableList<int>.Empty.min(cmp).shouldBeNone();
    }

    [Test]
    public void TestMax() {
      seq.max(cmp).shouldBeSome(seq.Max());
      ImmutableList<int>.Empty.max(cmp).shouldBeNone();
    }
  }
}