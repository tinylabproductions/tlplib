using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Data {
  public static class PrefValExts {
    /// <summary>
    /// You should not write directly to <see cref="PrefVal{A}"/> when using
    /// <see cref="IRxRef{A}"/>, because their states are only synchronized
    /// from ref to prefval. 
    /// </summary>
    public static RxRef<A> toRxRef<A>(this PrefVal<A> val) {
      var rx = new RxRef<A>(val.value);
      rx.subscribe(NoOpDisposableTracker.instance, v => val.value = v);
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