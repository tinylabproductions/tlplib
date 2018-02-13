using com.tinylabproductions.TLPLib.Functional;
using static com.tinylabproductions.TLPLib.Functional.F;

// Non-generated tuple extensions.

namespace System {
  public static class TupleExts {
    public static Tpl<AA, B> map1<A, AA, B>(this Tpl<A, B> t, Fn<A, AA> f) => F.t(f(t._1), t._2);
    public static Tpl<AA, B> map1<A, AA, B>(this Tpl<A, B> t, Fn<A, B, AA> f) => F.t(f(t._1, t._2), t._2);
    public static Tpl<A, BB> map2<A, B, BB>(this Tpl<A, B> t, Fn<B, BB> f) => F.t(t._1, f(t._2));
    public static Tpl<A, BB> map2<A, B, BB>(this Tpl<A, B> t, Fn<A, B, BB> f) => F.t(t._1, f(t._1, t._2));

    public static Tpl<A, B, C> flatten<A, B, C>(this Tpl<Tpl<A, B>, C> _) =>
      t(_._1._1, _._1._2, _._2);
    public static Tpl<Tpl<A, B>, C> unflatten<A, B, C>(this Tpl<A, B, C> t) =>
      F.t(F.t(t._1, t._2), t._3);

    public static Tpl<A, B, C, D> flatten<A, B, C, D>(this Tpl<Tpl<Tpl<A, B>, C>, D> _) =>
      t(_._1._1._1, _._1._1._2, _._1._2, _._2);
    public static Tpl<Tpl<Tpl<A, B>, C>, D> unflatten<A, B, C, D>(this Tpl<A, B, C, D> _) =>
      F.t(F.t(F.t(_._1, _._2), _._3), _._4);

    public static Tpl<A, B, C, D, E> flatten<A, B, C, D, E>(this Tpl<Tpl<Tpl<Tpl<A, B>, C>, D>, E> _) =>
      t(_._1._1._1._1, _._1._1._1._2, _._1._1._2, _._1._2, _._2);
    public static Tpl<A, B, C, D, E, F> flatten<A, B, C, D, E, F>(this Tpl<Tpl<Tpl<Tpl<Tpl<A, B>, C>, D>, E>, F> _) =>
      t(_._1._1._1._1._1, _._1._1._1._1._2, _._1._1._1._2, _._1._1._2, _._1._2, _._2);
  }
}