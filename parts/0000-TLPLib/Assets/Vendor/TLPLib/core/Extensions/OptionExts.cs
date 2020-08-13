using System;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;


namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI]
  public static class OptionExts {
    public static bool getOrLog<A>(
      this Option<A> opt, out A a, LogEntry msg, ILog log = null, LogLevel level = LogLevel.ERROR
    ) {
      if (!opt.valueOut(out a)) {
        log ??= Log.d;
        if (log.willLog(level)) log.log(level, msg);
        return false;
      }
      return true;
    }
    
    [VarMethodMacro(
@"if (!${opt}.valueOut(out var ${varName})) {
  if (${log}.willLog(${level})) ${log}.log(${level}, ${msg});
  return;
}")]
    public static A getOr_LOG_AND_RETURN<A>(
      this Option<A> opt, LogEntry msg, ILog log, LogLevel level
    ) => throw new MacroException();
    
    public static Option<B> flatMapUnity<A, B>(this Option<A> opt, Func<A, B> func) where B : class =>
      opt.isSome ? F.opt(func(opt.__unsafeGet)) : F.none<B>();
  }
}