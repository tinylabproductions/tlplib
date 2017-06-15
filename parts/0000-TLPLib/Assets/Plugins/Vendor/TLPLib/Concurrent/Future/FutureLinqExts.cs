using System;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class FutureLinqExts {
    public static Future<B> Select<A, B>(this Future<A> fa, Fn<A, B> f) => fa.map(f);
    public static Future<B> SelectMany<A, B>(this Future<A> fa, Fn<A, Future<B>> f) => fa.flatMap(f);
    public static Future<C> SelectMany<A, B, C>(this Future<A> fa, Fn<A, Future<B>> f, Fn<A, B, C> g) => 
      fa.flatMap(f, g);
  }
}