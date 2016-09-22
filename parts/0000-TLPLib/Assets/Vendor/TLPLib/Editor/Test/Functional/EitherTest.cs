using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class EitherTestEquality {
    [Test]
    public void WhenLeftEquals() {
      Either<int, string>.Left(0).shouldEqual(Either<int, string>.Left(0));
      Either<int, string>.Left(10).shouldEqual(Either<int, string>.Left(10));
    }

    [Test]
    public void WhenRightEquals() {
      Either<int, string>.Right("0").shouldEqual(Either<int, string>.Right("0"));
      Either<int, string>.Right("10").shouldEqual(Either<int, string>.Right("10"));
    }

    [Test]
    public void WhenNotEqual() {
      ImmutableList.Create(
        Either<int, string>.Left(0),
        Either<int, string>.Left(1),
        Either<int, string>.Right("0"),
        Either<int, string>.Right("1")
      ).shouldTestInequalityAgainst(ImmutableList.Create(
        Either<int, string>.Left(10),
        Either<int, string>.Left(11),
        Either<int, string>.Right("10"),
        Either<int, string>.Right("11")
      ));
    }
  }

  public class EitherTest {
    [Test]
    public void WhenHasOneError() {
      var l = ImmutableList.Create("error");
      new[] {
        Either<ImmutableList<string>, int>.Left(l),
        Either<ImmutableList<string>, int>.Right(4)
      }.sequence().shouldBeLeftEnum(l);
    }

    [Test]
    public void WhenHasMultipleErrors() {
      new[] {
        Either<ImmutableList<string>, int>.Left(ImmutableList.Create("error")),
        Either<ImmutableList<string>, int>.Left(ImmutableList.Create("error2"))
      }.sequence().shouldBeLeftEnum(ImmutableList.Create("error", "error2"));
    }

    [Test]
    public void WhenHasNoErrors() {
      new[] {
        Either<ImmutableList<string>, int>.Right(3),
        Either<ImmutableList<string>, int>.Right(4)
      }.sequence().shouldBeRightEnum(ImmutableList.Create(3, 4));
    }
  }

  public class EitherTestSwap {
    [Test] public void WhenLeft() => Either<int, string>.Left(3).swap.shouldBeRight(3);
    [Test] public void WhenRight() => Either<string, int>.Right(3).swap.shouldBeLeft(3);
  }

  public class EitherTestForeach {
    [Test]
    public void WhenLeft() {
      foreach (var _ in Either<int, string>.Left(3))
        Assert.Fail("It should not iterate if left");
    }

    [Test]
    public void WhenRight() {
      var called = 0;
      foreach (var b in Either<string, int>.Right(3)) {
        b.shouldEqual(3);
        called++;
      }
      called.shouldEqual(1, "it should yield once");
    }
  }
}
