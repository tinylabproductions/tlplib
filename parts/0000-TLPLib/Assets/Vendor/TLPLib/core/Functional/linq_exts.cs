using System;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class IOLinqExts {
    [PublicAPI] public static IO<B> Select<A, B>(this IO<A> t, Func<A, B> f) => t.map(f);
    [PublicAPI] public static IO<B> SelectMany<A, B>(this IO<A> t, Func<A, IO<B>> f) => t.flatMap(f);
    [PublicAPI] public static IO<B1> SelectMany<A, B, B1>(this IO<A> t, Func<A, IO<B>> f, Func<A, B, B1> g) =>
      t.flatMap(f, g);
  }

  public static class FutureLinqExts {
    [PublicAPI] public static Future<B> Select<A, B>(this Future<A> fa, Func<A, B> f) => fa.map(f);

    [PublicAPI] public static Future<B> SelectMany<A, B>(this Future<A> fa, Func<A, Future<B>> f) =>
      fa.flatMap(f);

    [PublicAPI] public static Future<C> SelectMany<A, B, C>(
      this Future<A> fa, Func<A, Future<B>> f, Func<A, B, C> g
    ) => fa.flatMap(f, g);
  }

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