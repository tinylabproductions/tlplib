using com.tinylabproductions.TLPLib.Logger;
using pzd.lib.log;
using GenerationAttributes;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;


namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class EitherExts {
    public static Option<B> getOrLog<A, B>(
      this Either<A, B> either, string errorMessage = null, object context = null, ILog log = null
    ) {
      if (either.isLeft) {
        log ??= Log.@default;
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

    /// <summary>
    /// If Either is Right - return the value.
    /// If Either is Left - log the provided message and return from scope.
    /// </summary>
    [VarMethodMacro(
@"var ${varName}__either = ${either};
if (!${varName}__either.rightValueOut(out var ${varName})) {
  var ${varName}__log = ${log};
  var ${varName}__level = ${level};
  if (${varName}__log.willLog(${varName}__level)) ${varName}__log.log(${varName}__level, ${message});
  return;
}")]
    public static B rightOr_LOG_MSG_AND_RETURN<A, B>(
      this Either<A, B> either, ILog log, string message, LogLevel level = LogLevel.ERROR
    ) => throw new MacroException();
    
    /// <summary>
    /// If Either is Right - return the value.
    /// If Either is Left - log the provided message and left value turned to string and return from scope.
    /// </summary>
    [VarMethodMacro(
@"var ${varName}__either = ${either};
if (!${varName}__either.rightValueOut(out var ${varName})) {
  var ${varName}__log = ${log};
  var ${varName}__level = ${level};
  if (${varName}__log.willLog(${varName}__level)) {
    string ${varName}__msg = ${message};
    if (${varName}__msg == null) ${varName}__msg = ${varName}__either.__unsafeGetLeft.ToString();
    else ${varName}__msg = $""{${varName}__msg}: {${varName}__either.__unsafeGetLeft}"";
    ${varName}__log.log(${varName}__level, ${varName}__msg);
  }
  return;
}")]
    public static B rightOr_LOG_LEFT_AND_RETURN<A, B>(
      this Either<A, B> either, ILog log, string message = null, LogLevel level = LogLevel.ERROR
    ) => throw new MacroException();
  }
}