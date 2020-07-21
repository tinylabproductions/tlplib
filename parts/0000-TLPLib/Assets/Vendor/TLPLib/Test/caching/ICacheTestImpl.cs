using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Filesystem;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.caching {
  public class ICacheTestImpl<A> : ICache<A> {
    readonly Dictionary<string, ICachedBlobTestImpl<A>> caches =
      new Dictionary<string, ICachedBlobTestImpl<A>>();

    public ICachedBlob<A> blobFor(string name) =>
      caches.getOrUpdate(name, () => new ICachedBlobTestImpl<A>(name));

    public Try<IEnumerable<PathStr>> files => F.scs(Enumerable.Empty<PathStr>());
  }
}