using System;
using com.tinylabproductions.TLPLib.Functional;
using System.Collections.Generic;
using System.Linq;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class AnyExts {
    /**
     * Useful in marking object creation just for side effects.
     *
     * For example:
     * `new GamePlaceBinding.Init(item, iconF, buttonSetting.name).forSideEffects();`
     **/
    public static void forSideEffects<A>(this A a) {}

    public static B mapVal<A, B>(this A any, Func<A, B> mapper) => mapper(any);
    public static To upcast<From, To>(this From any) where From : To => any;
    // ReSharper disable once UnusedParameter.Global
    /// <summary>
    /// Safely upcasts type. Uses example parameter to infer To. 
    /// </summary>
    public static To upcast<From, To>(this From any, To example) where From : To => any;
    public static IEnumerable<To> upcast<From, To>(this IEnumerable<From> any) where From : To
      => any.Select(_ => _.upcast<From, To>());

    public static A tap<A>(this A any, Action<A> tapper) {
      tapper(any);
      return any;
    }

    public static Option<A> opt<A>(this A a) where A : class => F.opt(a);
    public static Option<A> some<A>(this A a) => F.some(a);

    public static A orElseIfNull<A>(this A a, Func<A> ifNull) where A : class =>
      F.isNull(a) ? ifNull() : a;

    public static A orElseIfNull<A>(this A a, A ifNull) where A : class =>
      F.isNull(a) ? ifNull : a;
  }
}
