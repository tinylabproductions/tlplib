using System.Collections.Immutable;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class OneOfTestEquality {
    [Test]
    public void WhenNotEqual() {
      var set1 = ImmutableList.Create(
        new OneOf<int, string, bool>(4),
        new OneOf<int, string, bool>("foobar"),
        new OneOf<int, string, bool>(false)
      );
      var set2 = ImmutableList.Create(
        new OneOf<int, string, bool>(5),
        new OneOf<int, string, bool>("foo"),
        new OneOf<int, string, bool>(true)
      );

      set1.shouldTestInequalityAgainst(set2);
    }

    [Test]
    public void WhenAEqual() {
      new OneOf<int, string, bool>(0).shouldEqual(new OneOf<int, string, bool>(0));
      new OneOf<int, string, bool>(4).shouldEqual(new OneOf<int, string, bool>(4));
    }

    [Test]
    public void WhenBEqual() {
      new OneOf<int, string, bool>("bar").shouldEqual(new OneOf<int, string, bool>("bar"));
      new OneOf<int, string, bool>("foo").shouldEqual(new OneOf<int, string, bool>("foo"));
    }

    [Test]
    public void WhenCEqual() {
      new OneOf<int, string, bool>(true).shouldEqual(new OneOf<int, string, bool>(true));
      new OneOf<int, string, bool>(false).shouldEqual(new OneOf<int, string, bool>(false));
    }
  }
}