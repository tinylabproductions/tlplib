using System.Linq;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Collection {
  [TestFixture]
  public class CarouselEmitterTest {
    [Test]
    public void TestOneElement() {
      var ce = new CarouselEmitter<string>(new[] {F.t("a", 1)});
      var actual = ce.Take(5).ToArray();
      var expected = new[] {"a", "a", "a", "a", "a"};
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestTwoElements() {
      var ce = new CarouselEmitter<string>(new[] {F.t("a", 1), F.t("b", 1)});
      var actual = ce.Take(5).ToArray();
      var expected = new[] {"a", "b", "a", "b", "a"};
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void MaxCount1() {
      var ce = new CarouselEmitter<string>(new[] { F.t("a", 1), F.t("b", 1) });
      Assert.AreEqual(1, ce.maxCount);
    }

    [Test]
    public void MaxCount2() {
      var ce = new CarouselEmitter<string>(new[] {F.t("a", 1), F.t("b", 3)});
      Assert.AreEqual(3, ce.maxCount);
    }

    [Test]
    public void TotalCount1() {
      var ce = new CarouselEmitter<string>(new[] { F.t("a", 1), F.t("b", 1) });
      Assert.AreEqual(2, ce.totalCount);
    }

    [Test]
    public void TotalCount2() {
      var ce = new CarouselEmitter<string>(new[] {F.t("a", 1), F.t("b", 3)});
      Assert.AreEqual(4, ce.totalCount);
    }

    [Test]
    public void TestDifferentWeightsElements() {
      var ce = new CarouselEmitter<string>(new[] {F.t("a", 1), F.t("b", 3)});

      var actual = ce.Take(5).ToArray();
      var expected = new[] { "a", "b", "b", "b", "a" };
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestMultiples() {
      var ce = new CarouselEmitter<string>(
        new[] { F.t("a", 1), F.t("b", 3), F.t("a", 2) }
      );

      var actual = ce.Take(12).ToArray();
      var expected = new[] {
        "a", "b", "a",
        "b", "a", 
        "b", 
        "a", "b", "a",
        "b", "a",
        "b"
      };
      Assert.AreEqual(expected, actual);
    }
  }
}
