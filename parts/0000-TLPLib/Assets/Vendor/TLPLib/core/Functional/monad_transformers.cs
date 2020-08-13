using System;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Functional {
  /// <summary>
  /// C# does not have higher kinded types, so we need to specify every combination of monads here
  /// as an extension method. However this reduces visual noise in the usage sites.
  /// </summary>
  [PublicAPI] public static class MonadTransformers {
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