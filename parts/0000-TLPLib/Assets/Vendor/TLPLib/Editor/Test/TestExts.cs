using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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

    public static void shouldEqual<A>(this HashSet<A> a, HashSet<A> expected, string message=null) {
      Assert.That(a, new SetEquals<A>(expected), message);
    }

    public static void shouldBeSome<A>(this Option<A> a, A expected, string message=null) {
      a.shouldEqual(expected.some(), message);
    }

    public static void shouldBeNone<A>(this Option<A> a, string message=null) {
      a.shouldEqual(F.none<A>(), message);
    }

    public static void shouldBeLeft<A, B>(this Either<A, B> either, A expected, string message = null) {
      either.shouldEqual(F.left<A, B>(expected), message);
    }

    public static void shouldBeRight<A, B>(this Either<A, B> either, B expected, string message = null) {
      either.shouldEqual(F.right<A, B>(expected), message);
    }
  }

  public class SetEquals<A> : Constraint {
    public readonly HashSet<A> expected;

    public SetEquals(HashSet<A> expected) { this.expected = expected; }

    public override bool Matches(object actualO) {
      return F.opt(actualO as HashSet<A>).fold(false, expected.SetEquals);
    }

    public override void WriteDescriptionTo(MessageWriter writer) {
      writer.WriteExpectedValue(expected);
      writer.WriteActualValue(actual);
    }
  }
}
