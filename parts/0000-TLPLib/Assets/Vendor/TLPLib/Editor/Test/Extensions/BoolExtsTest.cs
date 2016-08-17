using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class BoolExtsTestToInt {
    [Test]
    public void WhenTrue() => true.toInt().shouldEqual(1);

    [Test]
    public void WhenFalse() => false.toInt().shouldEqual(0);
  }
}