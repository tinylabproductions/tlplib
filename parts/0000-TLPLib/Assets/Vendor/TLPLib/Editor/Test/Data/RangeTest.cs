using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  class RangeTestEnumerator {
    [Test]
    public void WhenHasElements() {
      var l = F.emptyList<int>();
      foreach (var i in new Range(0, 5)) l.Add(i);
      l.shouldEqual(F.list(0, 1, 2, 3, 4, 5));
    }

    [Test]
    public void WhenHasElementsMinValue() {
      var l = F.emptyList<int>();
      foreach (var i in new Range(int.MinValue, int.MinValue + 1)) l.Add(i);
      l.shouldEqual(F.list(int.MinValue, int.MinValue + 1));
    }

    [Test]
    public void WhenHasElementsMaxValue() {
      var l = F.emptyList<int>();
      foreach (var i in new Range(int.MaxValue - 1, int.MaxValue)) l.Add(i);
      l.shouldEqual(F.list(int.MaxValue - 1, int.MaxValue));
    }

    [Test]
    public void WhenNoElements() {
      var l = F.emptyList<int>();
      foreach (var i in new Range(0, -1)) l.Add(i);
      l.shouldBeEmpty();
    }
  }
}
