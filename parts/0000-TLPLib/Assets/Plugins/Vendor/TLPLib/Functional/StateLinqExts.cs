using System;

namespace com.tinylabproductions.TLPLib.Functional {
  /***
   * Extensions for state functions for linq expressions.
   **/
  public static class StateLinqExts {
    // map
    public static Fn<S, Tpl<S, B>> Select<S, A, B>(
      this Fn<S, Tpl<S, A>> stateFn, Fn<A, B> f
    ) => state => stateFn(state).map2(f);

    // bind/flatMap
    public static Fn<S, Tpl<S, C>> SelectMany<S, A, B, C>(
      this Fn<S, Tpl<S, A>> stateFn,
      Fn<A, Fn<S, Tpl<S, B>>> f,
      Fn<A, B, C> mapper
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