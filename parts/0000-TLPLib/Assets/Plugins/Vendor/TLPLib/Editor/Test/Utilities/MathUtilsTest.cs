using NUnit.Framework;
using com.tinylabproductions.TLPLib.Utilities;

namespace com.tinylabproductions.TLPLib.Editor.Test.Utilities {
  [TestFixture]
  public class MathUtilsTest {
    [Test]
    public void StartOfTheRange() {
      var actual = 5f.remap(5, 6, 0, 1);
      var expected = 0;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void EndOfTheRange() {
      var actual = 6f.remap(5, 6, 0, 1);
      var expected = 1;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void BetweenStartAndEndOfTheRange() {
      var actual = 5.5f.remap(5, 6, 0, 1);
      var expected = .5;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void LowerThanStartOfTheRange() {
      var actual = 4.5f.remap(5, 6, 0, 1);
      var expected = -.5;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void HigherThanEndOfTheRange() {
      var actual = 6.5f.remap(5, 6, 0, 1);
      var expected = 1.5;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void RangesAreSame() {
      var actual = .5f.remap(0, 1, 0, 1);
      var expected = .5;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void RangesAreSame2() {
      var actual = -1.5f.remap(0, 1, 0, 1);
      var expected = -1.5;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void RangesAreSame3() {
      var actual = 1.5f.remap(0, 1, 0, 1);
      var expected = 1.5;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReversedFirstRange() {
      var actual = 3f.remap(5, 2, 8, 20);
      var expected = 16f;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReversedSecondRange() {
      var actual = 2f.remap(3, 6, 24, 12);
      var expected = 28f;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void ReversedBothRanges() {
      var actual = 7f.remap(7, 2, 14, 4);
      var expected = 14f;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Zero() {
      var actual = 0f.remap(1, 2, 4, 5);
      var expected = 3f;
      Assert.AreEqual(expected, actual);
    }
  }
}
