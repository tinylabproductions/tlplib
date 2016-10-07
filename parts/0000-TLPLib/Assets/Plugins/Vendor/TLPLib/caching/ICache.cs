using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.caching {
  public interface ICache<A> {
    ICachedBlob<A> blobFor(string name);
  }

  public static class ICacheExts {
    public static ICache<B> bimap<A, B>(this ICache<A> cache, BiMapper<A, B> bimap) =>
      new ICacheMapper<A,B>(cache, bimap);
  }

  class ICacheMapper<A, B> : ICache<B> {
    readonly ICache<A> backing;
    readonly BiMapper<A, B> bimap;

    public ICacheMapper(ICache<A> backing, BiMapper<A, B> bimap) {
      this.backing = backing;
      this.bimap = bimap;
    }

    public ICachedBlob<B> blobFor(string name) => 
      backing.blobFor(name).bimap(bimap);
  }
}