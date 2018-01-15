using System;
using System.Collections;
using com.tinylabproductions.TLPLib.Functional;
using System.Collections.Generic;
using System.Linq;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class AnyExts {
    /** 
     * Useful in marking object creation just for side effects.
     *
     * For example:
     * `new GamePlaceBinding.Init(item, iconF, buttonSetting.name).forSideEffects();`
     **/
    public static void forSideEffects<A>(this A a) {}

    public static B mapVal<A, B>(this A any, Fn<A, B> mapper) => mapper(any);
    public static To upcast<From, To>(this From any) where From : To => any;
    public static IEnumerable<To> upcast<From, To>(this IEnumerable<From> any) where From : To 
      => any.Select(_ => _.upcast<From, To>());

    public static A tap<A>(this A any, Act<A> tapper) {
      tapper(any);
      return any;
    }

    public static Option<A> opt<A>(this A a) where A : class => F.opt(a);
    public static Option<A> some<A>(this A a) => F.some(a);

    public static A orElseIfNull<A>(this A a, Fn<A> ifNull) where A : class =>
      F.isNull(a) ? ifNull() : a;

    public static A orElseIfNull<A>(this A a, A ifNull) where A : class =>
      F.isNull(a) ? ifNull : a;

    public static CastBuilder<A> cast<A>(this A a) where A : class => new CastBuilder<A>(a);

    public static string asDebugString<A>(this A a) {
      // strings are enumrables, but we don't want to look at them like that...
      if (a is string) return $"'{a}'";
      var enumerable = a as IEnumerable;
      // ReSharper disable once InvokeAsExtensionMethod
      return enumerable != null 
        ? IEnumerableExts.asDebugString(enumerable) 
        : a == null ? "null" : a.ToString();
    }
  }

  public struct CastBuilder<From> where From : class {
    public readonly From from;

    public CastBuilder(From from) { this.from = from; }

    public Either<string, To> toE<To>() where To : class, From {
      var to = from as To;
      return to == null
        ? Either<string, To>.Left(errorMsg<To>())
        : Either<string, To>.Right(to);
    }

    string errorMsg<To>() => $"Can't cast {from.GetType()} to {typeof(To)}";

    public To to<To>() where To : class, From {
      var to = from as To;
      if (to == null) throw new InvalidCastException(errorMsg<To>());
      return to;
    }
  }
}
