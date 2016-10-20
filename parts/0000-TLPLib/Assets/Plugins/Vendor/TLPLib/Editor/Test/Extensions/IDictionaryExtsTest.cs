using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Extensions {
  [TestFixture]
  public class IDictionaryExtsTestGetOrUpdate {
    [Test]
    public void Test() {
      var dictionary = new Dictionary<int,string>();
      dictionary.getOrUpdate(1, () => "one").shouldEqual("one");
      dictionary.getOrUpdate(1, () => "two").shouldEqual("one");
    }
  }
}