using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  class RangeTest : ImplicitSpecification {
    [Test]
    public void enumerator() => describe(() => {
      when["has elements"] = () => {
        it["should enumerate through all of them"] = () =>
          new Range(0, 5).ToList().shouldEqual(F.list(0, 1, 2, 3, 4, 5));

        when["at min value edge"] = () => {
          it["should work"] = () =>
            new Range(int.MinValue, int.MinValue + 1).ToList()
            .shouldEqual(F.list(int.MinValue, int.MinValue + 1));
        };

        when["at max value edge"] = () => {
          it["should work"] = () =>
            new Range(int.MaxValue - 1, int.MaxValue).ToList()
            .shouldEqual(F.list(int.MaxValue - 1, int.MaxValue));
        };
      };

      when["range is empty"] = () => {
        it["should not yield any values"] = () => new Range(0, -1).shouldBeEmpty();
      };
    });
  }

  class URangeTestEnumerator {
    [Test]
    public void WhenHasElements() =>
      new URange(0, 5).ToList().shouldEqual(F.list(0u, 1u, 2u, 3u, 4u, 5u));

    [Test]
    public void WhenHasElementsMinValue() =>
      new URange(uint.MinValue, uint.MinValue + 1).ToList()
      .shouldEqual(F.list(uint.MinValue, uint.MinValue + 1));

    [Test]
    public void WhenHasElementsMaxValue() =>
      new URange(uint.MaxValue - 1, uint.MaxValue).ToList()
      .shouldEqual(F.list(uint.MaxValue - 1, uint.MaxValue));

    [Test]
    public void WhenNoElements() =>
      new URange(1, 0).shouldBeEmpty();
  }
}
