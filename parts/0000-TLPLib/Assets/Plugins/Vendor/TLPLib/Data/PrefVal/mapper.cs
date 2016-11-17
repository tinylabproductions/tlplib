using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  class PrefValMapper<A, B> : PrefVal<B> {
    readonly PrefVal<A> backing;
    readonly BiMapper<A, B> bimap;

    public PrefValMapper(PrefVal<A> backing, BiMapper<A, B> bimap) {
      this.backing = backing;
      this.bimap = bimap;
    }

    public bool cached => backing.cached;
    Option<Try<B>> ICachedBlob<B>.read() => backing.read().map(t => t.map(bimap.map));
    public Try<Unit> store(B data) => backing.store(bimap.comap(data));
    public Try<Unit> clear() => backing.clear();

    public B value {
      get { return bimap.map(backing.value); }
      set { backing.value = bimap.comap(value); }
    }

    public void forceSave() => backing.forceSave();
  }
}