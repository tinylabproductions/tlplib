using System;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional.higher_kinds {
  public interface Functor<Witness> {
    [PublicAPI]
    HigherKind<Witness, B> map<A, B>(HigherKind<Witness, A> data, Func<A, B> mapper);
  }

  public static class FunctorExts {
    [PublicAPI] public static HigherKind<Witness, B> map<Witness, A, B>(
      this HigherKind<Witness, A> hkt, Functor<Witness> F, Func<A, B> mapper
    ) => F.map(hkt, mapper);
  }
  
  [PublicAPI]
  public class Functors : Functor<Id.W>, Functor<Future.W>, Functor<Option.W> {
    [PublicAPI] public static readonly Functors i = new Functors();
    Functors() {}
    
    public HigherKind<Id.W, B> map<A, B>(HigherKind<Id.W, A> data, Func<A, B> mapper) =>
      Id.a(mapper(data.narrowK().a));

    public HigherKind<Future.W, B> map<A, B>(HigherKind<Future.W, A> data, Func<A, B> mapper) =>
      data.narrowK().map(mapper);

    public HigherKind<Option.W, B> map<A, B>(HigherKind<Option.W, A> data, Func<A, B> mapper) =>
      data.narrowK().map(mapper);
  }
}