using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Concurrent {
  static class FT {
    public static readonly Fn<int, Either<int, string>> left = F.left<int, string>;
    public static readonly Fn<string, Either<int, string>> right = F.right<int, string>;

    public static IEnumerable<Future<A>> addUnfullfilled<A>(this IEnumerable<Future<A>> futures)
      { return futures.Concat(Future.unfullfiled<A>().Yield()); }

    public static void shouldBeSuccessful<A>(this Future<A> f, A a) {
      f.type.shouldEqual(FutureType.Successful);
      f.value.shouldBeSome(a, "it should have a value");
    }

    public static void shouldBeUnfullfilled<A>(this Future<A> f, string message=null) {
      f.type.shouldEqual(FutureType.Unfullfilled, message);
      f.value.shouldBeNone($"{message ?? ""}: it shouldn't have a value");
    }
  }

  public class FutureTestOnComplete {
    const int value = 1;

    [Test]
    public void WhenSuccesful() {
      var f = Future.successful(value);
      var result = 0;
      f.onComplete(i => result = i);
      result.shouldEqual(value, "it should run the function immediately");
    }

    [Test]
    public void WhenUnfullfilled() {
      var f = Future.unfullfiled<int>();
      var result = 0;
      f.onComplete(i => result = i);
      result.shouldEqual(0, "it should not run the function");
    }

    [Test]
    public void WhenASync() {
      Promise<int> p;
      var f = Future<int>.async(out p);

      var result = 0;
      f.onComplete(i => result = i);
      result.shouldEqual(0, "it should not run the function immediately");
      p.complete(value);
      result.shouldEqual(value, "it run the function after application");
    }
  }

  public class FutureTestMap {
    readonly Fn<int, int> mapper = i => i * 2;

    [Test]
    public void WhenSuccessful() {
      Future<int>.successful(1).map(mapper).shouldBeSuccessful(2);
    }

    [Test]
    public void WhenUnfullfilled() {
      Future<int>.unfullfilled.map(mapper).shouldBeUnfullfilled();
    }

    [Test]
    public void WhenASync() {
      Promise<int> p;
      var f = Future<int>.async(out p);
      var f2 = f.map(mapper);
      f2.type.shouldEqual(FutureType.ASync);
      f2.value.shouldBeNone("it should not have value before original future completion");
      p.complete(1);
      f2.value.shouldBeSome(2, "it should have value after original future completion");
    }
  }

  public class FutureTestFlatMap {
    readonly Fn<int, Future<int>> successfulMapper = i => Future.successful(i * 2);
    readonly Fn<int, Future<int>> unfullfilledMapper = i => Future<int>.unfullfilled;

    readonly Future<int>
      successful = Future.successful(1),
      unfullfilled = Future<int>.unfullfilled;

    [Test]
    public void SuccessfulToSuccessful() {
      successful.flatMap(successfulMapper).shouldBeSuccessful(2);
    }
    [Test]
    public void SuccessfulToUnfullfilled() {
      successful.flatMap(unfullfilledMapper).shouldBeUnfullfilled();
    }
    [Test]
    public void SuccessfulToASync() {
      Promise<int> p2;
      var f2 = Future<int>.async(out p2);
      var f = successful.flatMap(_ => f2);
      f.type.shouldEqual(FutureType.ASync);
      f.value.shouldBeNone("it should be uncompleted if source future is incomplete");
      p2.complete(2);
      f.value.shouldBeSome(2, "it should complete after completing the source future");
    }

    [Test]
    public void UnfullfilledToSuccessful() {
      unfullfilledShouldNotCallMapper(successfulMapper);
    }
    [Test]
    public void UnfullfilledToUnfullfilled() {
      unfullfilledShouldNotCallMapper(unfullfilledMapper);
    }
    [Test]
    public void UnfullfilledToASync() {
      unfullfilledShouldNotCallMapper(i => Future.a<int>(p => {}));
    }

    void unfullfilledShouldNotCallMapper<A>(Fn<int, Future<A>> mapper) {
      var called = false;
      unfullfilled.flatMap(i => {
        called = true;
        return mapper(i);
      }).shouldBeUnfullfilled();
      called.shouldBeFalse("it should not call the mapper");
    }

    [Test]
    public void ASyncToSuccessful() {
      Promise<int> p;
      var f = Future<int>.async(out p);
      var called = false;
      var f2 = f.flatMap(i => {
        called = true;
        return Future.successful(i);
      });
      f2.type.shouldEqual(FutureType.ASync);
      f2.value.shouldBeNone();
      called.shouldBeFalse("it should not call function until completion of a source promise");
      p.complete(1);
      called.shouldBeTrue();
      f2.value.shouldBeSome(1);
    }

    [Test]
    public void ASyncToUnfullfiled() {
      Promise<int> p;
      var f = Future<int>.async(out p);
      var called = false;
      var f2 = f.flatMap(_ => {
        called = true;
        return Future<int>.unfullfilled;
      });
      f2.type.shouldEqual(FutureType.ASync);
      f2.value.shouldBeNone();
      called.shouldBeFalse();
      p.complete(1);
      called.shouldBeTrue();
      f2.value.shouldBeNone("it shouldn't complete even if source future is completed");
    }

    [Test]
    public void ASyncToASync() {
      Promise<int> p1;
      var f1 = Future<int>.async(out p1);
      Promise<int> p2;
      var f2 = Future<int>.async(out p2);

      var called = false;
      var f = f1.flatMap(_ => {
        called = true;
        return f2;
      });
      f.type.shouldEqual(FutureType.ASync);
      f.value.shouldBeNone("it should be not completed at start");
      called.shouldBeFalse();
      p1.complete(1);
      called.shouldBeTrue();
      f.value.shouldBeNone("it should be not completed if source future completes");
      p2.complete(2);
      f.value.shouldBeSome(2, "it should be completed");
    }
  }

  public class FutureTestZip {
    [Test]
    public void WhenEitherSideUnfullfilled() {
      foreach (var t in new[] {
        F.t("X-O", Future.unfullfiled<int>(), Future.successful(1)),
        F.t("O-X", Future.successful(1), Future.unfullfiled<int>())
      }) t.ua((name, fa, fb) => fa.zip(fb).shouldBeUnfullfilled(name));
    }

    [Test]
    public void WhenBothSidesSuccessful() {
      Future.successful(1).zip(Future.successful(2)).shouldBeSuccessful(F.t(1, 2));
    }

    [Test]
    public void WhenASync() {
      whenASync(true);
      whenASync(false);
    }

    static void whenASync(bool completeFirst) {
      Promise<int> p1, p2;
      var f1 = Future<int>.async(out p1);
      var f2 = Future<int>.async(out p2);
      var f = f1.zip(f2);
      f.type.shouldEqual(FutureType.ASync);
      f.value.shouldBeNone();
      (completeFirst ? p1 : p2).complete(completeFirst ? 1 : 2);
      f.value.shouldBeNone("it should not complete just from one side");
      (completeFirst ? p2 : p1).complete(completeFirst ? 2 : 1);
      f.value.shouldBeSome(F.t(1, 2), "it should complete from both sides");
    }
  }

  public class FutureTestFirstOf {
    [Test]
    public void WhenHasCompleted() {
      new[] {
        Future.unfullfiled<int>(),
        Future.unfullfiled<int>(),
        Future.successful(1),
        Future.unfullfiled<int>(),
        Future.unfullfiled<int>()
      }.firstOf().value.get.shouldEqual(1);
    }

    [Test]
    public void WhenHasMultipleCompleted() {
      new[] {
        Future.unfullfiled<int>(),
        Future.unfullfiled<int>(),
        Future.successful(1),
        Future.unfullfiled<int>(),
        Future.successful(2),
        Future.unfullfiled<int>()
      }.firstOf().value.get.shouldEqual(1);
    }

    [Test]
    public void WhenNoCompleted() {
      new[] {
        Future.unfullfiled<int>(),
        Future.unfullfiled<int>(),
        Future.unfullfiled<int>(),
        Future.unfullfiled<int>()
      }.firstOf().value.shouldEqual(F.none<int>());
    }
  }

  public class FutureTestFirstOfWhere {
    [Test]
    public void ItemFound() {
      new[] {1, 3, 5, 6, 7}.
        Select(Future.successful).firstOfWhere(i => (i % 2 == 0).opt(i)).
        value.get.shouldEqual(6.some());
    }
    [Test]
    public void MultipleItemsFound() {
      new[] {1, 3, 5, 6, 7, 8}.
        Select(Future.successful).firstOfWhere(i => (i % 2 == 0).opt(i)).
        value.get.shouldEqual(6.some());
    }

    [Test]
    public void ItemNotFound() {
      new[] {1, 3, 5, 7}.
        Select(Future.successful).firstOfWhere(i => (i % 2 == 0).opt(i)).
        value.get.shouldBeNone();
    }

    [Test]
    public void ItemNotFoundNotCompleted() {
      new[] {1, 3, 5, 7}.Select(Future.successful).addUnfullfilled().
        firstOfWhere(i => (i % 2 == 0).opt(i)).
        value.shouldBeNone();
    }
  }

  public class FutureTestFirstOfSuccessful {
    [Test]
    public void RightFound() {
      new[] { FT.left(1), FT.left(3), FT.left(5), FT.right("6"), FT.left(7) }.
        Select(Future.successful).firstOfSuccessful().
        value.get.shouldBeSome("6");
    }

    [Test]
    public void MultipleRightsFound() {
      new[] { FT.left(1), FT.left(3), FT.left(5), FT.right("6"), FT.left(7), FT.right("8") }.
        Select(Future.successful).firstOfSuccessful().
        value.get.shouldBeSome("6");
    }

    [Test]
    public void RightNotFound() {
      new[] { FT.left(1), FT.left(3), FT.left(5), FT.left(7) }.
        Select(Future.successful).firstOfSuccessful().
        value.get.shouldBeNone();
    }

    [Test]
    public void RightNotFoundNoComplete() {
      new[] { FT.left(1), FT.left(3), FT.left(5), FT.left(7) }.
        Select(Future.successful).addUnfullfilled().firstOfSuccessful().
        value.shouldBeNone();
    }
  }

  public class FutureTestFirstOfSuccessfulCollect {
    [Test]
    public void ItemFound() {
      new [] { FT.left(1), FT.left(2), FT.right("a"), FT.left(3) }.
        Select(Future.successful).firstOfSuccessfulCollect().value.get.
        shouldEqual(F.right<int[], string>("a"));
    }

    [Test]
    public void MultipleItemsFound() {
      new [] { FT.left(1), FT.left(2), FT.right("a"), FT.left(3), FT.right("b") }.
        Select(Future.successful).firstOfSuccessfulCollect().value.get.
        shouldEqual(F.right<int[], string>("a"));
    }

    [Test]
    public void ItemNotFound() {
      new [] { FT.left(1), FT.left(2), FT.left(3), FT.left(4) }.
        Select(Future.successful).firstOfSuccessfulCollect().value.get.
        leftValue.get.asString().shouldEqual(new[] { 1, 2, 3, 4 }.asString());
    }

    [Test]
    public void ItemNotFoundNoCompletion() {
      new [] { FT.left(1), FT.left(2), FT.left(3), FT.left(4) }.
        Select(Future.successful).addUnfullfilled().
        firstOfSuccessfulCollect().value.shouldEqual(F.none<Either<int[], string>>());
    }
  }
}