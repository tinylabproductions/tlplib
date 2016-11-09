using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class UIntExtsTestAddClamped {
    [Test]
    public void WithZero() {
      const int b = 0;
      testAdd(uint.MinValue, b);
      testAdd(1, b);
      testAdd(15456u, b);
    }

    [Test]
    public void WithPositive() {
      const int b = 1, b1 = 100;
      testAdd(uint.MinValue, b);
      testAdd(uint.MinValue, b1);
      testAdd(1, b);
      testAdd(1, b1);
      testAdd(15456u, b);
      testAdd(15456u, b1);
      uint.MaxValue.addClamped(b).shouldEqual(uint.MaxValue);
      (uint.MaxValue - b1 + 1).addClamped(b1).shouldEqual(uint.MaxValue);
      (uint.MaxValue - b1 - 1).addClamped(b1).shouldEqual(uint.MaxValue - 1);
    }

    [Test]
    public void WithNegative() {
      const int b = -1;
      uint.MinValue.addClamped(b).shouldEqual(uint.MinValue);
      1u.addClamped(b).shouldEqual(0u);
      15456u.addClamped(b).shouldEqual(15455u);
      uint.MaxValue.addClamped(b).shouldEqual(uint.MaxValue - 1);
    }

    static void testAdd(uint a, uint b) => a.addClamped((int) b).shouldEqual(a + b);
  }
}