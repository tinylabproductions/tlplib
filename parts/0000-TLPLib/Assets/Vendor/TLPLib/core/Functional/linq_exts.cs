using System;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class StateLinqExts {
    // map
    [PublicAPI] public static Func<S, Tpl<S, B>> Select<S, A, B>(
      this Func<S, Tpl<S, A>> stateFn, Func<A, B> f
    ) => state => stateFn(state).map2(f);

    // bind/flatMap
    [PublicAPI] public static Func<S, Tpl<S, C>> SelectMany<S, A, B, C>(
      this Func<S, Tpl<S, A>> stateFn,
      Func<A, Func<S, Tpl<S, B>>> f,
      Func<A, B, C> mapper
    ) => state => {
      var t1 = stateFn(state);
      var newState = t1._1;
      var a = t1._2;

      var t2 = f(a)(newState);
      var newState2 = t2._1;
      var b = t2._2;

      var c = mapper(a, b);
      return F.t(newState2, c);
    };
  }

}