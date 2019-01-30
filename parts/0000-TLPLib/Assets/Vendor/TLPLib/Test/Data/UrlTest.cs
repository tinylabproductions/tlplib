using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  public class UrlTestSlashOperator {
    [Test]
    public void WhenEndsWithSlash() =>
      (new Url("foo/") / "bar").shouldEqual(new Url("foo/bar"));

    [Test]
    public void WhenDoesNotEndWithSlash() =>
      (new Url("foo") / "bar").shouldEqual(new Url("foo/bar"));
  }
}