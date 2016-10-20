using System.Text;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.caching {
  public interface ICachedBlob {
    bool cached { get; }
    Try<Unit> clear();
  }

  public interface ICachedBlob<A> : ICachedBlob {
    /**
     * None if not cached.
     * Some(Success(...)) on successful read.
     * Some(Failure(...)) on failed read.
     **/
    Option<Try<A>> read();
    Try<Unit> store(A data);
  }

  class ICachedBlobMapper<A, B> : ICachedBlob<B> {
    readonly ICachedBlob<A> backing;
    readonly BiMapper<A, B> bimap;

    public ICachedBlobMapper(ICachedBlob<A> backing, BiMapper<A, B> bimap) {
      this.backing = backing;
      this.bimap = bimap;
    }

    public bool cached => backing.cached;
    public Try<Unit> clear() => backing.clear();
    public Option<Try<B>> read() => backing.read().map(_ => _.map(bimap.map));
    public Try<Unit> store(B data) => backing.store(bimap.comap(data));
  }

  public static class ICachedBlobExts {
    public static ICachedBlob<B> bimap<A, B>(
      this ICachedBlob<A> blob, BiMapper<A, B> bimap
    ) => new ICachedBlobMapper<A,B>(blob, bimap);

    public static ICachedBlob<string> toStringBlob(
      this ICachedBlob<byte[]> blob, Encoding encoding = null
    ) => blob.bimap(BiMapper.byteArrString(encoding));
  }
}