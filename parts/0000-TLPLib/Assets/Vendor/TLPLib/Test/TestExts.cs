using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Configuration;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Data.typeclasses;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Formats.MiniJSON;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace com.tinylabproductions.TLPLib.Test {
  [PublicAPI] public class TestBase {
    protected readonly IDisposableTracker tracker = new DisposableTracker();

    [TearDown]
    public void CleanupIDisposableTracker() => tracker.Dispose();

    public static void testCollection() {
      WeakReference reference = null;
      new Action(() => {
        var service = new object();
        reference = new WeakReference(service, true);
      })();

      // Service should have gone out of scope about now,
      // so the garbage collector can clean it up
      new object().forSideEffects();
      GC.Collect();
      GC.WaitForPendingFinalizers();

      Assert.IsNull(reference.Target);
    }

    public static void shouldBeIdentical<A>(A a1, A a2) {
      a1.shouldEqual(a2);
      a2.shouldEqual(a1);
      a1.GetHashCode().shouldEqual(a2.GetHashCode());
    }

    public static void shouldNotEqualSymmetrical<A>(A a1, A a2) {
      a1.shouldNotEqual(a2);
      a2.shouldNotEqual(a1);
    }

    public static Fn<Unit> code(Action a) => () => {
      a();
      return F.unit;
    };

    public static Fn<A> code<A>(Fn<A> a) => a;
  }

  [PublicAPI] public static class TestExts {
    public static void shouldBeEmpty(this IEnumerable enumerable, string message = null) =>
      Assert.IsEmpty(enumerable, message);

    public static void shouldBeEmpty<A>(
      this ICollection<A> enumerable, Fn<ICollection<A>, string> message = null
    ) =>
      Assert.IsEmpty(enumerable, message?.Invoke(enumerable));

    public static void shouldNotBeEmpty(this IEnumerable enumerable, string message = null) {
      Assert.IsNotEmpty(enumerable, message);
    }

    public static void shouldBeTrue(this bool b, string message = null) {
      Assert.True(b, message);
    }

    public static void shouldBeFalse(this bool b, string message = null) {
      Assert.False(b, message);
    }

    public static void shouldEqual<A>(this A a, A expected, string message=null) {
      if (a is IEnumerable ae && expected is IEnumerable ee) {
        ae.shouldEqualEnum(ee, message);
      }
      else {
        Assert.AreEqual(expected, a, message);
      }
    }

    public static void shouldNotEqual<A>(this A a, A expected, string message=null) {
      if (a is IEnumerable ae && expected is IEnumerable ee) {
        ae.shouldNotEqualEnum(ee, message);
      }
      else {
        Assert.AreNotEqual(expected, a, message);
      }
    }

    public static void shouldRefEqual<A>(
      this A a, A expected, string message = null
    ) where A : class {
      if (!ReferenceEquals(a, expected)) Assert.Fail(
        message ??
        $"Expected expected={expected} and actual={a} to be the same reference, but they were not."
      );
    }

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

    public static void shouldContain<A>(
      this IEnumerable<A> enumerable, Fn<A, bool> predicate, string message = null
    ) {
      if (enumerable.find(predicate).isNone) Assert.Fail(
        message ??
        $"Expected enumerable to contain {typeof(A)} which matches predicate, but nothing was found."
      );
    }

    public static void shouldNotContain<A>(
      this IEnumerable<A> enumerable, Fn<A, bool> predicate,
      Fn<A, string> message = null
    ) {
      foreach (var a in enumerable.find(predicate)) Assert.Fail(
        message?.Invoke(a) ??
        $"Expected enumerable not to contain {typeof(A)} which matches predicate, but {a} was found."
      );
    }

    public static void shouldTestInequalityAgainst<A>(this IEnumerable<A> set1, IEnumerable<A> set2) {
      foreach (var i1 in set1)
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var i2 in set2) {
          i1.Equals(i2).shouldBeFalse($"{i1} should != {i2}, but they were treated as equal");
          i2.Equals(i1).shouldBeFalse($"{i2} should != {i1}, but they were treated as equal");
        }
    }

    public static void shouldEqual<A>(this HashSet<A> a, HashSet<A> expected, string message=null) {
      Assert.AreEqual(expected, a, message);
    }

    public static void shouldEqualEnum<A>(
      this IEnumerable<A> a, IEnumerable<A> expected, string message = null
    ) => shouldEqualEnum((IEnumerable) a, expected, message);

    public static void shouldEqualEnum(
      this IEnumerable a, IEnumerable expected, string message = null
    ) => CollectionAssert.AreEquivalent(expected, a, message);

    public static void shouldEqualEnum<A>(this IEnumerable<A> a, params A[] expected) =>
      shouldEqualEnum(a, expected, null);

    public static void shouldNotEqualEnum<A>(
      this IEnumerable<A> a, IEnumerable<A> expected, string message = null
    ) => shouldNotEqualEnum((IEnumerable) a, expected, message);

    public static void shouldNotEqualEnum(
      this IEnumerable a, IEnumerable expected, string message = null
    ) => CollectionAssert.AreNotEquivalent(expected, a, message);

    public static void shouldNotEqualEnum<A>(this IEnumerable<A> a, params A[] expected) =>
      shouldNotEqualEnum(a, expected, null);

    public static void shouldMatch<A>(this A a, Fn<A, bool> predicate, string message = null) {
      if (! predicate(a))
        failWithPrefix(message, $"Expected {a} to match predicate, but it didn't");
    }

    public static void shouldBeSome<A>(this Option<A> maybeA, A expected, string message=null) {
      if (maybeA.valueOut(out var a)) a.shouldEqual(expected, message ?? $"Expected {maybeA} to be {F.some(expected)}");
      else Assert.Fail(message ?? $"Expected to be Some({expected}, but it was None");
    }

    public static void shouldBeAnySome<A>(this Option<A> a, string message = null) {
      if (a.isNone) Assert.Fail(message ?? $"Expected {a} to be Some!");
    }

    public static void shouldBeSomeEnum<E>(this Option<E> aOpt, E expected, string message = null)
      where E : IEnumerable
    {
      foreach (var a in aOpt) {
        a.shouldEqualEnum(expected);
        return;
      }
      aOpt.shouldBeSome(expected, message);
    }

    public static void shouldBeNone<A>(this Option<A> a, string message=null) {
      a.shouldEqual(F.none<A>(), message);
    }

    public static void shouldBeLeft<A, B>(this Either<A, B> either, string message = null) {
      if (! either.isLeft) Assert.Fail(
        message ?? 
        $"Expected either to be left, but it was Right({either.__unsafeGetRight.asDebugString()})!"
      );
    }

    public static void shouldBeLeft<A, B>(this Either<A, B> either, A expected, string message = null) {
      either.shouldEqual(F.left<A, B>(expected), message);
    }

    public static void shouldBeLeftEnum<A, B>(
      this Either<A, B> either, A expected, string message = null
    ) where A : IEnumerable {
      foreach (var a in either.leftValue) {
        a.shouldEqualEnum(expected, message);
        return;
      }
      either.shouldBeLeft(message);
    }

    public static void shouldBeRight<A, B>(this Either<A, B> either, string message = null) {
      if (! either.isRight) Assert.Fail(
        message ?? 
        $"Expected either to be right, but it was Left({either.__unsafeGetLeft.asDebugString()})!"
      );
    }

    public static void shouldBeRight<A, B>(this Either<A, B> either, B expected, string message = null) =>
      either.shouldEqual(F.right<A, B>(expected), message);

    public static void shouldBeRightEnum<A, B>(
      this Either<A, B> either, B expected, string message = null
    ) where B : IEnumerable {
      foreach (var b in either.rightValue) {
        b.shouldEqualEnum(expected, message);
        return;
      }
      either.shouldBeRight(message);
    }

    public static void shouldBeError<A>(this Try<A> _try, Type exceptionType=null, string message = null) {
      void fail(string msg) => failWithPrefix(message, msg);
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
      this string json, Config.Parser<object, A> parser
    ) =>
      $@"{{""item"": {json}}}".asConfig().eitherGet("item", parser);

    public static ChangeMatcher<A, R> shouldChange<A, R>(
      this Fn<R> fn, Fn<A> measure, Numeric<A> num
    ) => new ChangeMatcher<A, R>(fn, measure, num);

    public static ChangeMatcher<A, R> shouldChange<A, R>(
      this Fn<R> fn, Val<A> measure, Numeric<A> num
    ) => new ChangeMatcher<A, R>(fn, () => measure.value, num);

    public static void shouldNotChange<A, R>(
      this Fn<R> fn, Fn<A> measure, Numeric<A> num
    ) => fn.shouldChange(measure, num).by(0);

    public static void shouldNotChange<A, R>(
      this Fn<R> fn, Val<A> measure, Numeric<A> num
    ) => fn.shouldChange(measure, num).by(0);

    public static ChangeMatcher<int, R> shouldChange<R>(this Fn<R> fn, Fn<int> measure) =>
      fn.shouldChange(measure, Numeric.integer);

    public static ChangeMatcher<int, R> shouldChange<R>(this Fn<R> fn, Val<int> measure) =>
      fn.shouldChange(measure, Numeric.integer);

    public static void shouldNotChange<R>(this Fn<R> fn, Fn<int> measure) =>
      fn.shouldChange(measure).by(0);

    public static void shouldNotChange<R>(this Fn<R> fn, Val<int> measure) =>
      fn.shouldChange(measure).by(0);

    public static StreamMatcher<A> shouldPushTo<A>(
      this Action act, IRxObservable<A> obs
    ) => new StreamMatcher<A>(act, obs);
  }

  public class ChangeMatcher<MeasurementType, ReturnedType> {
    readonly Fn<ReturnedType> run;
    readonly Fn<MeasurementType> measure;
    readonly Numeric<MeasurementType> num;

    public ChangeMatcher(Fn<ReturnedType> run, Fn<MeasurementType> measure, Numeric<MeasurementType> num) {
      this.run = run;
      this.measure = measure;
      this.num = num;
    }

    public ReturnedType by(int i, string message = null) {
      var change = num.fromInt(i);
      var initial = measure();
      var ret = run();
      var after = measure();
      var actualChange = num.subtract(after, initial);

      if (message == null) {
        message =
          i == 0
          ? $"value should have not been changed, but it was changed " +
            $"from {initial} to {after} by {actualChange}"
          : $"value should have been changed from {initial} to {num.add(initial, change)} " +
            $"by {change}, but it was changed to {after} by {actualChange}";
      }

      num.subtract(after, initial).shouldEqual(change, message);
      return ret;
    }
  }

  /// <summary>
  /// More specialized ChangeMatcher
  /// For use when we are not only interested in the amount of callback calls
  /// But also in the results of those calls
  /// </summary>
  /// <typeparam name="A"></typeparam>
  public class StreamMatcher<A> {
    readonly Action act;
    readonly IRxObservable<A> obs;

    public StreamMatcher(Action act, IRxObservable<A> obs) {
      this.act = act;
      this.obs = obs;
    }

    public void resultIn(params A[] vals) => resultIn(vals, message: null);

    public void resultIn(ICollection<A> match, string message = null) {
      var streamValues = new List<A>();
      var sub = obs.subscribe(NoOpDisposableTracker.instance, streamValues.Add);
      act();
      sub.unsubscribe();

      if (message == null) {
        message = match.Any()
          ? $"call results do not match expected results "
          : $"there should have been no calls, but they still occured ";
        message += $"expected {match} received {streamValues}";
      }

      streamValues.shouldEqualEnum(match, message);
    }
  }
}
