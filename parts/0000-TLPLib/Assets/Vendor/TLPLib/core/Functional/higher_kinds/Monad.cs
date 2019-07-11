using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Functional.higher_kinds {
  public interface Monad<Witness> : Functor<Witness> {
    /// <summary>Wrap value in a monad context.</summary>
    [PublicAPI] HigherKind<Witness, A> point<A>(A a);
    [PublicAPI] HigherKind<Witness, B> flatMap<A, B>(
      HigherKind<Witness, A> data,
      Func<A, HigherKind<Witness, B>> mapper
    );
  }

  public class Monads : Monad<Id.W>, Monad<Future.W>, Monad<Option.W> {
    [PublicAPI] public static readonly Monads i = new Monads();
    Monads() {}
    
    #region Id

    public HigherKind<Id.W, B> map<A, B>(HigherKind<Id.W, A> data, Func<A, B> mapper) =>
      Functors.i.map(data, mapper);
    
    HigherKind<Id.W, A> Monad<Id.W>.point<A>(A a) => Id.a(a);

    public HigherKind<Id.W, B> flatMap<A, B>(
      HigherKind<Id.W, A> data,
      Func<A, HigherKind<Id.W, B>> mapper
    ) => mapper(data.narrowK().a); 

    #endregion

    #region Future

    public HigherKind<Future.W, B> map<A, B>(HigherKind<Future.W, A> data, Func<A, B> mapper) =>
      Functors.i.map(data, mapper);

    HigherKind<Future.W, A> Monad<Future.W>.point<A>(A a) =>
      Future.successful(a);
    
    public HigherKind<Future.W, B> flatMap<A, B>(
      HigherKind<Future.W, A> data, Func<A, HigherKind<Future.W, B>> mapper
    ) => data.narrowK().flatMap(a => mapper(a).narrowK()); 

    #endregion

    #region Option

    public HigherKind<Option.W, B> map<A, B>(HigherKind<Option.W, A> data, Func<A, B> mapper) =>
      Functors.i.map(data, mapper);

    public HigherKind<Option.W, A> point<A>(A a) =>
      F.some(a);

    public HigherKind<Option.W, B> flatMap<A, B>(
      HigherKind<Option.W, A> data,
      Func<A, HigherKind<Option.W, B>> mapper
    ) => data.narrowK().flatMap(a => mapper(a).narrowK());

    #endregion
  }

  public static class MonadExts {
    /// <summary>Sequences any monad in higher kinded form.</summary>
    [PublicAPI]
    public static HigherKind<W, ImmutableList<A>> sequence<A, W>(
      this IEnumerable<HigherKind<W, A>> enumerable, Monad<W> M
    ) {
      var builderHKT = enumerable.Aggregate(
        M.point(ImmutableList.CreateBuilder<A>()),
        (listBuilderHKT, aHKT) => M.flatMap(listBuilderHKT, listBuilder => M.map(aHKT, a => {
          listBuilder.Add(a);
          return listBuilder;
        }))
      );
      return M.map(builderHKT, _ => _.ToImmutable());
    }
  }
}