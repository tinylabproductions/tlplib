using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class EitherExts {
    public static Option<B> getOrLog<A, B>(
      this Either<A, B> either, string errorMessage = null, object context = null, ILog log = null
    ) {
      if (either.isLeft) {
        log = log ?? Log.@default;
        log.error(
          errorMessage == null 
          ? either.__unsafeGetLeft.ToString() 
          : $"{errorMessage}: {either.__unsafeGetLeft}",
          context
        );
      }
      return either.rightValue;
    }

    public static B getOrLogAndDefault<A, B>(
      this Either<A, B> either, B defaultValue, string errorMessage = null, object context = null, ILog log = null
    ) {
      var opt = either.getOrLog(errorMessage, context, log);
      return opt.valueOut(out var b) ? b : defaultValue;
    }
  }
}