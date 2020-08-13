using System.Text;
using pzd.lib.test_framework;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class BiMapperTestString {
    [Test]
    public void TestString() {
      var bm = BiMapper.byteArrString(Encoding.UTF8).reverse;
      const string str = "UTF8 string: ąčęėįšųž";
      bm.comap(bm.map(str)).shouldEqual(str, "mapping & comapping shouldn't change the value");
    }
  }
}