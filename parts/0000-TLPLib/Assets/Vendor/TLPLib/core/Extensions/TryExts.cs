using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class TryExts {
    public static Option<A> getOrLog<A>(this Try<A> t, string errorMessage, object context = null, ILog log = null) {
      if (t.isError) {
        log = log ?? Log.@default;
        log.error(errorMessage, t.__unsafeException, context);
      }
      return Some.a(t.__unsafeGet);
    }
  }
}