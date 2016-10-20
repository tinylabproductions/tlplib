using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.caching {
  public class ICacheTestImpl<A> : ICache<A> {
    readonly Dictionary<string, ICachedBlobTestImpl<A>> caches = 
      new Dictionary<string, ICachedBlobTestImpl<A>>();

    public ICachedBlob<A> blobFor(string name) => 
      caches.getOrUpdate(name, () => new ICachedBlobTestImpl<A>(name));
  }
}