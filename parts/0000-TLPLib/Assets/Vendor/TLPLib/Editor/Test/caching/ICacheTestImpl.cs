namespace com.tinylabproductions.TLPLib.caching {
  public class ICacheTestImpl<A> : ICache<A> {
    public ICachedBlob<A> blobFor(string name) => new ICachedBlobTestImpl<A>(name);
  }
}