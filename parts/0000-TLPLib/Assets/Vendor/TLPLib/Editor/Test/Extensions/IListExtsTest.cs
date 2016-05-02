using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  public class IListTestHeadOption {
    [Test]
    public void Test() {
      F.emptyList<int>().headOption().shouldBeNone();
      F.list(0).headOption().shouldBeSome(0);
      F.list(0, 1).headOption().shouldBeSome(0);
    }
  }
}