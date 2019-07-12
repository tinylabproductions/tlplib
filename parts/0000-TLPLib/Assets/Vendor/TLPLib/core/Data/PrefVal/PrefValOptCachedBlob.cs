using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using GenerationAttributes;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Data {
  [Record]
  partial class PrefValOptCachedBlob<A> : ICachedBlob<A> {
    readonly PrefVal<Functional.Option<A>> backing;

    public bool cached => backing.value.isSome;
    public Functional.Option<Try<A>> read() => backing.value.map(F.scs);
    public Try<Unit> store(A data) => backing.store(data.some());
    public Try<Unit> clear() => backing.store(Functional.Option<A>.None);
  }
}