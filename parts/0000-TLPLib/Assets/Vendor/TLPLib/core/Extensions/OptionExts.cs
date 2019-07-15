using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using JetBrains.Annotations;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI]
  public static class OptionExts {
    public static bool getOrLog<A>(
      this Option<A> opt, out A a, LogEntry msg, ILog log = null, Log.Level level = Log.Level.ERROR
    ) {
      if (!opt.valueOut(out a)) {
        log = log ?? Log.d;
        if (log.willLog(level)) log.log(level, msg);
        return false;
      }
      return true;
    }
    
    public static Option<B> flatMapUnity<A, B>(this Option<A> opt, Func<A, B> func) where B : class =>
      opt.isSome ? F.opt(func(opt.__unsafeGet)) : F.none<B>();
  }
}