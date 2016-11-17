using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Data {
  public static class PrefValExts {
    // You should not write to Val when using RxRef
    public static RxRef<A> toRxRef<A>(this PrefVal<A> val) {
      var rx = new RxRef<A>(val.value);
      rx.subscribe(v => val.value = v);
      return rx;
    }

    public static PrefVal<B> bimap<A, B>(
      this PrefVal<A> val, BiMapper<A, B> bimap
    ) => new PrefValMapper<A, B>(val, bimap);

    public static ICachedBlob<A> optToCachedBlob<A>(
      this PrefVal<Option<A>> val
    ) => new PrefValOptCachedBlob<A>(val);
  }
}