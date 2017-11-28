using System;
using com.tinylabproductions.TLPLib.Concurrent;

namespace com.tinylabproductions.TLPLib.Functional {
  public static class OptionLinqExts {
    public static Option<B> Select<A, B>(this Option<A> opt, Fn<A, B> f) => opt.map(f);
    public static Option<B> SelectMany<A, B>(this Option<A> opt, Fn<A, Option<B>> f) => opt.flatMap(f);
    public static Option<C> SelectMany<A, B, C>(
      this Option<A> opt, Fn<A, Option<B>> f, Fn<A, B, C> g
    ) => opt.flatMap(f, g);
    public static Option<A> Where<A>(this Option<A> opt, Fn<A, bool> f) => opt.filter(f);
  }

  public static class EitherLinqExts {
    public static Either<L, R1> Select<L, R, R1>(this Either<L, R> e, Fn<R, R1> f) =>
      e.mapRight(f);

    public static Either<L, R1> SelectMany<L, R, R1>(this Either<L, R> e, Fn<R, Either<L, R1>> f) =>
      e.flatMapRight(f);

    public static Either<L, R2> SelectMany<L, R, R1, R2>(
      this Either<L, R> opt, Fn<R, Either<L, R1>> f, Fn<R, R1, R2> g
    ) => opt.flatMapRight(f, g);
  }

  public static class TryLinqExts {
    public static Try<B> Select<A, B>(this Try<A> t, Fn<A, B> f) => t.map(f);
    public static Try<B> SelectMany<A, B>(this Try<A> t, Fn<A, Try<B>> f) => t.flatMap(f);
    public static Try<B1> SelectMany<A, B, B1>(this Try<A> t, Fn<A, Try<B>> f, Fn<A, B, B1> g) => 
      t.flatMap(f, g);
  }

  public static class IOLinqExts {
    public static IO<B> Select<A, B>(this IO<A> t, Fn<A, B> f) => t.map(f);
    public static IO<B> SelectMany<A, B>(this IO<A> t, Fn<A, IO<B>> f) => t.flatMap(f);
    public static IO<B1> SelectMany<A, B, B1>(this IO<A> t, Fn<A, IO<B>> f, Fn<A, B, B1> g) => 
      t.flatMap(f, g);
  }

  public static class FutureLinqExts {
    public static Future<B> Select<A, B>(this Future<A> fa, Fn<A, B> f) => fa.map(f);

    public static Future<B> SelectMany<A, B>(this Future<A> fa, Fn<A, Future<B>> f) => 
      fa.flatMap(f);

    public static Future<C> SelectMany<A, B, C>(
      this Future<A> fa, Fn<A, Future<B>> f, Fn<A, B, C> g
    ) => fa.flatMap(f, g);
  }

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