using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
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
}
