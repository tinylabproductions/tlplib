using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Utilities {
  public class ArrayUtilsTestEnsureNotNull {
    [Test]
    public void WhenNotNull() {
      var original = new[] {"foo"};
      var actual = original.ensureNotNull();
      (original == actual).shouldBeTrue("be same object");
    }

    [Test]
    public void WhenNull() {
      var actualStr = ((string[]) null).ensureNotNull();
      actualStr.shouldEqual(new string[0]);

      var actualInt = ((int[]) null).ensureNotNull();
      actualInt.shouldEqual(new int[0]);
    }
  }
}