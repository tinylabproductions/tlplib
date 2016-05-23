using System.IO;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Filesystem {
  class PathStrTestConstructor {
    [Test]
    public void ItShouldNormalizeDirectorySeparators() {
      var expected = Path.Combine(Path.Combine("foo", "bar"), "baz");
      new PathStr("foo/bar/baz").path.shouldEqual(expected);
      new PathStr("foo\\bar\\baz").path.shouldEqual(expected);
    }
  }

  class PathStrTestEnsureBeginsWith {
    [Test]
    public void WhenDoesNotBegin() {
      new PathStr("foo/bar/baz")
        .ensureBeginsWith(new PathStr("lol/php")).shouldEqual(new PathStr("lol/php/foo/bar/baz"));
    }

    [Test]
    public void WhenItBeginsWith() {
      new PathStr("foo/bar/baz")
        .ensureBeginsWith(new PathStr("foo/bar")).shouldEqual(new PathStr("foo/bar/baz"));
    }
  }
}
