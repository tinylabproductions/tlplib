using System;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data.typeclasses {
  public class ComparableTest {
    static readonly ImmutableList<int> seq = ImmutableList.Create(1, 6, 2, 5, 2, 7, 2, 19, -1, -5);
    static readonly Comparable<int> cmp = Comparable.integer;
    static readonly ImmutableList<Tpl<string, int>> seqTpl =
      seq.Select(v => F.t($"foo{v}", v)).ToImmutableList();
    static readonly Fn<Tpl<string, int>, int> extract = _ => _._2;

    [Test]
    public void TestMin() {
      seq.min(cmp).shouldBeSome(seq.Min());
      ImmutableList<int>.Empty.min(cmp).shouldBeNone();
    }

    [Test]
    public void TestMinBy() {
      seqTpl.minBy(cmp, extract).shouldBeSome(seqTpl[seq.IndexOf(seq.Min())]);
      ImmutableList<Tpl<string, int>>.Empty.minBy(cmp, extract).shouldBeNone();
    }

    [Test]
    public void TestMax() {
      seq.max(cmp).shouldBeSome(seq.Max());
      ImmutableList<int>.Empty.max(cmp).shouldBeNone();
    }

    [Test]
    public void TestMaxBy() {
      seqTpl.maxBy(cmp, extract).shouldBeSome(seqTpl[seq.IndexOf(seq.Max())]);
      ImmutableList<Tpl<string, int>>.Empty.maxBy(cmp, extract).shouldBeNone();
    }
  }
}