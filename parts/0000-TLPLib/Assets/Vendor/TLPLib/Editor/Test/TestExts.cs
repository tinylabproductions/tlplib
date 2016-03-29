using System.Collections;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Test {
  public static class TestExts {
    public static void shouldBeEmpty(this IEnumerable enumerable, string message = null) {
      Assert.IsEmpty(enumerable, message);
    }

    public static void shouldBeTrue(this bool b, string message = null) {
      Assert.True(b, message);
    }

    public static void shouldBeFalse(this bool b, string message = null) {
      Assert.False(b, message);
    }

    public static void shouldEqual<A>(this A a, A expected, string message=null) {
      Assert.AreEqual(expected, a, message);
    }

    public static void shouldBeSome<A>(this Option<A> a, A expected, string message=null) {
      a.shouldEqual(expected.some(), message);
    }

    public static void shouldBeNone<A>(this Option<A> a, string message=null) {
      a.shouldEqual(F.none<A>(), message);
    }
  }
}
