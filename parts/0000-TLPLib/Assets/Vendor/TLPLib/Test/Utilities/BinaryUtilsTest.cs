using pzd.lib.test_framework;
using NUnit.Framework;
using pzd.lib.test_framework.spec;
using pzd.lib.utils;

namespace com.tinylabproductions.Test.Utilities {
  public class BinaryUtilsTest : ImplicitSpecification {
    [Test]
    public void intBigEndian() => describe(() => {
      var buf = new byte[6];
      for (var offset_ = 0; offset_ <= 2; offset_++) {
        var offset = offset_;
        foreach (var i in new[] {int.MinValue, -213, -12, -1, 0, 1, 12, 213, int.MaxValue}) {
          it[$"should convert {i} at offset {offset} back and forth"] = () => {
            buf.WriteIntBigEndian(i, offset);
            buf.ReadIntBigEndian(offset).shouldEqual(i);
          };
        }
      }
    });
  }
}