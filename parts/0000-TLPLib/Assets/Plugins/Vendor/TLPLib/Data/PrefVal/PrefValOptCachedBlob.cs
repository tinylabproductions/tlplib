using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  class PrefValOptCachedBlob<A> : ICachedBlob<A> {
    readonly PrefVal<Option<A>> backing;

    public PrefValOptCachedBlob(PrefVal<Option<A>> backing) { this.backing = backing; }

    public bool cached => backing.value.isSome;
    public Option<Try<A>> read() => backing.value.map(F.scs);
    public Try<Unit> store(A data) => backing.store(data.some());
    public Try<Unit> clear() => backing.store(Option<A>.None);
  }
}