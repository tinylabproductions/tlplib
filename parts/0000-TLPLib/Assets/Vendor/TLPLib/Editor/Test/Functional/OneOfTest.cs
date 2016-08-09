using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class OneOfTestEquality {
    [Test]
    public void WhenANotEqual() {
      new OneOf<int, string, bool>(4).shouldNotEqual(new OneOf<int, string, bool>(5));
    }

    [Test]
    public void WhenAEqual() {
      new OneOf<int, string, bool>(4).shouldEqual(new OneOf<int, string, bool>(4));
    }

    [Test]
    public void WhenBNotEqual() {
      new OneOf<int, string, bool>("foo").shouldNotEqual(new OneOf<int, string, bool>("bar"));
    }

    [Test]
    public void WhenBEqual() {
      new OneOf<int, string, bool>("foo").shouldEqual(new OneOf<int, string, bool>("foo"));
    }

    [Test]
    public void WhenCNotEqual() {
      new OneOf<int, string, bool>(true).shouldNotEqual(new OneOf<int, string, bool>(false));
    }

    [Test]
    public void WhenCEqual() {
      new OneOf<int, string, bool>(true).shouldEqual(new OneOf<int, string, bool>(true));
    }
  }
}