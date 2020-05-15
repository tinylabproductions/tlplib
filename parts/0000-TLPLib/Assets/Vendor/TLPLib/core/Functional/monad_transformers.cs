using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Functional {
  /// <summary>
  /// C# does not have higher kinded types, so we need to specify every combination of monads here
  /// as an extension method. However this reduces visual noise in the usage sites.
  /// </summary>
  [PublicAPI] public static class MonadTransformers {
    #region Option

    #region of Either

    public static Option<Either<A, BB>> mapT<A, B, BB>(
      this Option<Either<A, B>> m, Func<B, BB> mapper
    ) => m.map(_ => _.mapRight(mapper));

    public static Option<Either<A, BB>> flatMapT<A, B, BB>(
      this Option<Either<A, B>> m, Func<B, Either<A, BB>> mapper
    ) => m.map(_ => _.flatMapRight(mapper));

    public static Either<A, Option<B>> extract<A, B>(this Option<Either<A, B>> o) {
      if (o.isSome) {
        var e = o.__unsafeGet;
        return
          e.isLeft
            ? Either<A, Option<B>>.Left(e.__unsafeGetLeft)
            : Either<A, Option<B>>.Right(e.__unsafeGetRight.some());
      }
      return F.none<B>();
    }

    #endregion

    #region of IEnumerable

    public static IEnumerable<Option<A>> extract<A>(
      this Option<IEnumerable<A>> opt
    ) =>
      opt.isNone
        ? Enumerable.Empty<Option<A>>()
        : opt.__unsafeGet.Select(a => a.some());

    #endregion

    #endregion

    #region Either

    #region of Option

    public static Either<A, Option<BB>> mapT<A, B, BB>(
      this Either<A, Option<B>> m, Func<B, BB> mapper
    ) => m.mapRight(_ => _.map(mapper));

    public static Either<A, Option<BB>> flatMapT<A, B, BB>(
      this Either<A, Option<B>> m, Func<B, Option<BB>> mapper
    ) => m.mapRight(_ => _.flatMap(mapper));

    #endregion

    #endregion

    #region Future

    #region of Option

    public static Future<Option<B>> mapT<A, B>(
      this Future<Option<A>> m, Func<A, B> mapper
    ) => m.map(_ => _.map(mapper));

    public static Future<Option<B>> flatMapT<A, B>(
      this Future<Option<A>> m, Func<A, Option<B>> mapper
    ) => m.map(_ => _.flatMap(mapper));

    public static Future<Option<B>> flatMapT<A, B>(
      this Future<Option<A>> m, Func<A, Future<Option<B>>> mapper
    ) => m.flatMap(_ => _.fold(
      () => Future.successful(F.none<B>()),
      mapper
    ));

    #endregion

    #region of Either

    public static Future<Either<A, BB>> mapT<A, B, BB>(
      this Future<Either<A, B>> m, Func<B, BB> mapper
    ) => m.map(_ => _.mapRight(mapper));

    public static Future<Either<A, BB>> flatMapT<A, B, BB>(
      this Future<Either<A, B>> m, Func<B, Either<A, BB>> mapper
    ) => m.map(_ => _.flatMapRight(mapper));

    public static Future<Either<A, BB>> flatMapT<A, B, BB>(
      this Future<Either<A, B>> m, Func<B, Future<Either<A, BB>>> mapper
    ) => m.flatMap(_ => _.fold(
      a => Future.successful(Either<A, BB>.Left(a)),
      mapper
    ));

    public static Future<Either<A, BB>> flatMapT<B, BB, A>(
      this Future<Either<A, B>> m, Func<B, Future<BB>> mapper
    ) => m.flatMap(_ => _.fold(
      err => Future.successful(Either<A, BB>.Left(err)),
      from => mapper(from).map(Either<A, BB>.Right)
    ));


    #endregion

    #region of Try

    public static Future<Try<To>> flatMapT<From, To>(
      this Future<Try<From>> m, Func<From, Future<To>> mapper
    ) => m.flatMap(_ => _.fold(
      from => mapper(from).map(F.scs),
      err => Future.successful(F.err<To>(err))
    ));
    
    public static Future<Try<To>> flatMapT<From, To>(
      this Future<Try<From>> m, Func<From, Future<Try<To>>> mapper
    ) => m.flatMap(_ => _.fold(
      mapper,
      err => Future.successful(F.err<To>(err))
    ));

    #endregion

    #endregion

    #region LazyVal

    #region of Option

    [PublicAPI]
    public static LazyVal<Option<B>> lazyMapT<A, B>(
      this LazyVal<Option<A>> m, Func<A, B> mapper
    ) => m.lazyMap(_ => _.map(mapper));

    [PublicAPI]
    public static LazyVal<Option<B>> lazyFlatMapT<A, B>(
      this LazyVal<Option<A>> m, Func<A, Option<B>> mapper
    ) => m.lazyMap(_ => _.flatMap(mapper));

    #endregion

    #region of Try

    public static LazyVal<Try<B>> lazyMapT<A, B>(
      this LazyVal<Try<A>> m, Func<A, B> mapper
    ) => m.lazyMap(_ => _.map(mapper));

    #endregion

    #region of Future

    public static LazyVal<Future<B>> lazyMapT<A, B>(
      this LazyVal<Future<A>> m, Func<A, B> mapper
    ) => m.lazyMap(_ => _.map(mapper));

    #endregion

    #endregion

    #region IRxVal

    #region of Option

    public static IRxVal<Option<B>> mapT<A, B>(
      this IRxVal<Option<A>> rxMaybeA, Func<A, B> f
    ) => rxMaybeA.map(maybeA => maybeA.map(f));

    public static IRxObservable<B> flatMapT<A, B>(
      this IRxVal<Option<A>> rxMaybeA, Func<A, IRxObservable<B>> f
    ) => rxMaybeA.flatMap(maybeA => maybeA.valueOut(out var a) ? f(a) : Observable<B>.empty);

    public static IRxVal<Option<B>> flatMapT<A, B>(
      this IRxVal<Option<A>> rxMaybeA, Func<A, IRxVal<B>> f
    ) => rxMaybeA.flatMap(maybeA => maybeA.valueOut(out var a) ? f(a).map(Some.a) : RxVal.cached(Option<B>.None));

    #endregion

    #endregion
  }
}