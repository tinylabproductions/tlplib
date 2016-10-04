using System;
using System.Collections;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Configuration;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace com.tinylabproductions.TLPLib.Test {
  public class TestBase {
    public static void shouldEqual<A>(A a1, A a2) {
      a1.shouldEqual(a2);
      a2.shouldEqual(a1);
      a1.GetHashCode().shouldEqual(a2.GetHashCode());
    }

    public static void shouldNotEqual<A>(A a1, A a2) {
      a1.shouldNotEqual(a2);
      a2.shouldNotEqual(a1);
    }
  }

  public static class TestExts {
    public static void shouldBeEmpty(this IEnumerable enumerable, string message = null) {
      Assert.IsEmpty(enumerable, message);
    }

    public static void shouldNotBeEmpty(this IEnumerable enumerable, string message = null) {
      Assert.IsNotEmpty(enumerable, message);
    }

    public static void shouldBeTrue(this bool b, string message = null) {
      Assert.True(b, message);
    }

    public static void shouldBeFalse(this bool b, string message = null) {
      Assert.False(b, message);
    }

    public static void shouldEqual<A>(this A a, A expected, string message=null) => 
      Assert.AreEqual(expected, a, message);

    public static void shouldNotEqual<A>(this A a, A expected, string message=null) => 
      Assert.AreNotEqual(expected, a, message);

    public static void shouldBeApproximately(this int a, int target, int delta, string message = null) {
      var actualDelta = Math.Abs(a - target);
      if (actualDelta > delta) Assert.Fail(
        message ?? $"Expected {a} to be within {delta} of {target}, but the delta was {actualDelta}"
      );
    }

    public static void shouldInclude(this string s, string substring, string message = null) {
      if (!s.Contains(substring))
        Assert.Fail(
          $"\"{s}\" should include \"{substring}\", but it did not." + 
          F.opt(message).map(_ => $" {_}").getOrElse("")
        );
    }

    public static void shouldNotInclude(this string s, string substring, string message = null) {
      if (s.Contains(substring))
        Assert.Fail(
          $"\"{s}\" should not include \"{substring}\", but it did." + 
          F.opt(message).map(_ => $" {_}").getOrElse("")
        );
    }

    public static void shouldContain<A, C>(
      this C collection, A a, string message = null
    ) where C : ICollection, ICollection<A> =>
      Assert.Contains(a, collection, message);

    public static void shouldNotContain<A>(
      this IEnumerable<A> enumerable, Fn<A, bool> predicate, string message = null
    ) => 
      enumerable.collectFirst(a => predicate(a).opt(a)).voidFold(
        Assert.Pass,
        a => Assert.Fail(
          message ?? 
          $"Expected enumerable not to contain {typeof(A)} which matches predicate, but {a} was found."
        )
      );

    public static void shouldTestInequalityAgainst<A>(this IEnumerable<A> set1, IEnumerable<A> set2) {
      foreach (var i1 in set1)
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var i2 in set2) {
          i1.Equals(i2).shouldBeFalse($"{i1} should != {i2}, but they were treated as equal");
          i2.Equals(i1).shouldBeFalse($"{i2} should != {i1}, but they were treated as equal");
        }
    }

    public static void shouldEqual<A>(this HashSet<A> a, HashSet<A> expected, string message=null) {
      Assert.That(a, new SetEquals<A>(expected), message);
    }

    public static void shouldMatch<A>(this A a, Fn<A, bool> predicate, string message = null) {
      if (! predicate(a))
        failWithPrefix(message, $"Expected {a} to match predicate, but it didn't");
    }

    public static void shouldBeSome<A>(this Option<A> a, A expected, string message=null) {
      a.shouldEqual(expected.some(), message);
    }

    public static void shouldBeSomeEnum<A>(this Option<A> aOpt, A expected, string message = null)
      where A : IEnumerable
    {
      foreach (var a in aOpt) {
        a.shouldEqual(expected, message);
        return;
      }
      aOpt.shouldBeSome(expected, message);
    }

    public static void shouldBeNone<A>(this Option<A> a, string message=null) {
      a.shouldEqual(F.none<A>(), message);
    }

    public static void shouldBeLeft<A, B>(this Either<A, B> either, string message = null) {
      if (! either.isLeft) Assert.Fail(message ?? $"Expected {either} to be left!");
    }

    public static void shouldBeLeft<A, B>(this Either<A, B> either, A expected, string message = null) => 
      either.shouldEqual(F.left<A, B>(expected), message);

    public static void shouldBeLeftEnum<A, B>(
      this Either<A, B> either, A expected, string message = null
    ) where A : IEnumerable {
      foreach (var a in either.leftValue) {
        a.shouldEqual(expected, message);
        return;
      }
      either.shouldEqual(F.left<A, B>(expected), message);
    }

    public static void shouldBeRight<A, B>(this Either<A, B> either, string message = null) {
      if (! either.isRight) Assert.Fail(message ?? $"Expected {either} to be right!");
    }

    public static void shouldBeRight<A, B>(this Either<A, B> either, B expected, string message = null) => 
      either.shouldEqual(F.right<A, B>(expected), message);

    public static void shouldBeRightEnum<A, B>(
      this Either<A, B> either, B expected, string message = null
    ) where B : IEnumerable {
      foreach (var b in either.rightValue) {
        b.shouldEqual(expected, message);
        return;
      }
      either.shouldEqual(F.right<A, B>(expected), message);
    }

    public static void shouldBeError<A>(this Try<A> _try, Type exceptionType=null, string message = null) {
      Act<string> fail = msg => failWithPrefix(message, msg);
      _try.voidFold(
        _ => fail(
          $"Expected {_try} to be an error of {F.opt(exceptionType).fold("Exception", e => e.ToString())}"
        ),
        e => {
          if (exceptionType != null && !exceptionType.IsInstanceOfType(e))
            fail($"Expected {_try} to be of type {exceptionType}");
        }
      );
    }

    public static void shouldBeSuccess<A>(this Try<A> _try, A expected, string message = null) {
      // To allow deeper comparisons of collections.
      _try.voidFold(
        a => a.shouldEqual(expected, message),
        e => failWithPrefix(message, $"expected to be {F.scs(expected)}, was {e}")
      );
    }

    public static void shouldBeOfSuccessfulType<A>(this Future<A> f, A a, string message=null) {
      f.type.shouldEqual(FutureType.Successful);
      f.shouldBeCompleted(a, message);
    }

    public static void shouldBeOfUnfulfilledType<A>(this Future<A> f, string message=null) {
      f.type.shouldEqual(FutureType.Unfulfilled, message);
      f.value.shouldBeNone($"{message ?? ""}: it shouldn't have a value");
    }

    public static void shouldBeCompleted<A>(this Future<A> f, A a, string message=null) {
      f.value.voidFold(
        () => Assert.Fail($"{f} should be completed, but it wasn't".joinOpt(message)),
        v => v.shouldEqual(a, message)
      );
    }

    public static void shouldNotBeCompleted<A>(this Future<A> f, string message=null) {
      foreach (var v in f.value) Assert.Fail(
        $"{f} should not be completed, but it was completed with '{v}'".joinOpt(message)
      );
    }

    static void failWithPrefix(string prefix, string message) {
      Assert.Fail((prefix != null ? $"{prefix}\n" : "") + message);
    }

    public static IConfig asConfig(this string json) =>
      new Config(Json.Deserialize(json).cast().to<Dictionary<string, object>>());

    public static Either<ConfigLookupError, A> testParser<A>(
      this string json, Config.Parser<A> parser
    ) =>
      $@"{{""item"": {json}}}".asConfig().eitherGet("item", parser);
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
