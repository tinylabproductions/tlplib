using com.tinylabproductions.TLPLib.Functional;

// Non-generated tuple extensions.

namespace System {
  public static class TupleExts {
    public static Tpl<AA, B> map1<A, AA, B>(this Tpl<A, B> t, Fn<A, AA> f) => F.t(f(t._1), t._2);
    public static Tpl<AA, B> map1<A, AA, B>(this Tpl<A, B> t, Fn<A, B, AA> f) => F.t(f(t._1, t._2), t._2);
    public static Tpl<A, BB> map2<A, B, BB>(this Tpl<A, B> t, Fn<B, BB> f) => F.t(t._1, f(t._2));
    public static Tpl<A, BB> map2<A, B, BB>(this Tpl<A, B> t, Fn<A, B, BB> f) => F.t(t._1, f(t._1, t._2));
  }
}