using com.tinylabproductions.TLPLib.caching;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Data {
  public static class PrefValExts {
    public static ICachedBlob<A> optToCachedBlob<A>(
      this PrefVal<Option<A>> val
    ) => new PrefValOptCachedBlob<A>(val);
  }
}