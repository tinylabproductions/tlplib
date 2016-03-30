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