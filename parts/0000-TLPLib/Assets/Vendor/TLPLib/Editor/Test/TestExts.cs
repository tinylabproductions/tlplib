using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Test {
  public static class TestExts {
    public static void shouldBeTrue(this bool b, string message = null) {
      Assert.True(b, message);
    }

    public static void shouldBeFalse(this bool b, string message = null) {
      Assert.False(b, message);
    }

    public static void shouldEqual<A>(this A a, A expected, string message=null) {
      Assert.AreEqual(expected, a, message);
    }
  }
}
