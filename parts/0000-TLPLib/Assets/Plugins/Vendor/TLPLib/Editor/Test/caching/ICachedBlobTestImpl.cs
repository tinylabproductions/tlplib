using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.caching {
  public class ICachedBlobTestImpl<A> : ICachedBlob<A> {
    public readonly string name;

    public ICachedBlobTestImpl() : this("not set") { }
    public ICachedBlobTestImpl(string name) { this.name = name; }

    public Option<A> blob = Option<A>.None;

    public bool cached => blob.isDefined;

    public Option<Try<A>> read() => blob.map(F.scs);

    public Try<Unit> store(A data) {
      blob = data.some();
      return F.scs(F.unit);
    }

    public Try<Unit> clear() {
      blob = blob.none;
      return F.scs(F.unit);
    }
  }
}