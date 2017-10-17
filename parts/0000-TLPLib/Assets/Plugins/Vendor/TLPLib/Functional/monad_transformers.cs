using System;
using com.tinylabproductions.TLPLib.Concurrent;

namespace com.tinylabproductions.TLPLib.Functional {
  /// <summary>
  /// C# does not have higher kinded types, so we need to specify every combination of monads here
  /// as an extension method. However this reduces visual noise in the usage sites.
  /// </summary>
  public static class MonadTransformers {
    public static Option<Either<A, BB>> mapT<A, B, BB>(
      this Option<Either<A, B>> m, Fn<B, BB> mapper
    ) => m.map(_ => _.mapRight(mapper));

    public static Option<Either<A, BB>> flatMapT<A, B, BB>(
      this Option<Either<A, B>> m, Fn<B, Either<A, BB>> mapper
    ) => m.map(_ => _.flatMapRight(mapper));

    public static Either<A, Option<BB>> mapT<A, B, BB>(
      this Either<A, Option<B>> m, Fn<B, BB> mapper
    ) => m.mapRight(_ => _.map(mapper));

    public static Either<A, Option<BB>> flatMapT<A, B, BB>(
      this Either<A, Option<B>> m, Fn<B, Option<BB>> mapper
    ) => m.mapRight(_ => _.flatMap(mapper));

    public static Future<Option<B>> mapT<A, B>(
      this Future<Option<A>> m, Fn<A, B> mapper
    ) => m.map(_ => _.map(mapper));

    public static Future<Option<B>> flatMapT<A, B>(
      this Future<Option<A>> m, Fn<A, Option<B>> mapper
    ) => m.map(_ => _.flatMap(mapper));

    public static Future<Either<A, BB>> mapT<A, B, BB>(
      this Future<Either<A, B>> m, Fn<B, BB> mapper
    ) => m.map(_ => _.mapRight(mapper));

    public static Future<Either<A, BB>> flatMapT<A, B, BB>(
      this Future<Either<A, B>> m, Fn<B, Either<A, BB>> mapper
    ) => m.map(_ => _.flatMapRight(mapper));
  }
}