using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.caching {
  public class ICachedBlobTestBiMap {
    readonly ICachedBlob<int> blob;
    readonly ICachedBlob<string> mappedBlob;

    public ICachedBlobTestBiMap() {
      blob = new ICachedBlobTestImpl<int>();
      mappedBlob = blob.bimap(BiMapper.a<int, string>(i => i.ToString(), int.Parse));
    }

    [SetUp]
    public void SetUp() { blob.clear(); }

    [Test]
    public void TestCached() {
      blob.cached.shouldBeFalse();
      mappedBlob.cached.shouldBeFalse();
      blob.store(0);
      mappedBlob.cached.shouldBeTrue();
    }

    [Test]
    public void TestClear() {
      blob.store(0);
      blob.cached.shouldBeTrue();
      mappedBlob.clear();
      blob.cached.shouldBeFalse();
      mappedBlob.cached.shouldBeFalse();
    }

    [Test]
    public void TestStore() {
      mappedBlob.store("3");
      blob.read().get.getOrThrow.shouldEqual(3);
    }

    [Test]
    public void TestRead() {
      blob.store(5);
      mappedBlob.read().get.getOrThrow.shouldEqual("5");
    }
  }
}