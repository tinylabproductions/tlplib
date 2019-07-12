using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxValOps {
    public static IRxVal<B> map<A, B>(this IRxVal<A> src, Func<A, B> mapper) =>
      new RxVal<B>(
        mapper(src.value),
        setValue => src.subscribeWithoutEmit(
          NoOpDisposableTracker.instance, a => setValue(mapper(a))
        )
      );

    #region #filter

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Func<A, bool> predicate, Func<A> onFiltered) =>
      rx.map(filterMapper(predicate, onFiltered));

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Func<A, bool> predicate, A onFiltered) =>
      rx.map(filterMapper(predicate, onFiltered));

    #endregion

    public static IRxVal<B> flatMap<A, B>(this IRxVal<A> src, Func<A, IRxVal<B>> mapper) {
      var bRx = mapper(src.value);

      return new RxVal<B>(
        bRx.value,
        setValue => {
          var tracker = NoOpDisposableTracker.instance;
          var subToBRx = bRx.subscribeWithoutEmit(tracker, b => setValue(b));

          var aSub = src.subscribeWithoutEmit(
            tracker,
            a => {
              subToBRx.unsubscribe();
              bRx = mapper(a);
              setValue(bRx.value);
              subToBRx = bRx.subscribeWithoutEmit(tracker, b => setValue(b));
            }
          );
          return aSub.andThen(() => subToBRx.unsubscribe());
        }
      );
    }

    #region #zip

    public static IRxVal<R> zip<A1, A2, R>(
      this IRxVal<A1> a1Src, IRxVal<A2> a2Src, Func<A1, A2, R> zipper
    ) =>
      new RxVal<R>(
        zipper(a1Src.value, a2Src.value),
        setValue => {
          var tracker = NoOpDisposableTracker.instance;
          var a1Sub = a1Src.subscribeWithoutEmit(tracker, a1 => setValue(zipper(a1, a2Src.value)));
          var a2Sub = a2Src.subscribeWithoutEmit(tracker, a2 => setValue(zipper(a1Src.value, a2)));
          return a1Sub.join(a2Sub);
        }
      );

    public static IRxVal<R> zip<A1, A2, A3, R>(
      this IRxVal<A1> a1Src, IRxVal<A2> a2Src, IRxVal<A3> a3Src, Func<A1, A2, A3, R> zipper
    ) =>
      new RxVal<R>(
        zipper(a1Src.value, a2Src.value, a3Src.value),
        setValue => {
          var tracker = NoOpDisposableTracker.instance;
          var a1Sub = a1Src.subscribeWithoutEmit(tracker, a1 => setValue(zipper(a1, a2Src.value, a3Src.value)));
          var a2Sub = a2Src.subscribeWithoutEmit(tracker, a2 => setValue(zipper(a1Src.value, a2, a3Src.value)));
          var a3Sub = a3Src.subscribeWithoutEmit(tracker, a3 => setValue(zipper(a1Src.value, a2Src.value, a3)));
          return a1Sub.join(a2Sub, a3Sub);
        }
      );

    public static IRxVal<R> zip<A1, A2, A3, A4, R>(
      this IRxVal<A1> a1Src, IRxVal<A2> a2Src, IRxVal<A3> a3Src, IRxVal<A4> a4Src,
      Func<A1, A2, A3, A4, R> zipper
    ) =>
      new RxVal<R>(
        zipper(a1Src.value, a2Src.value, a3Src.value, a4Src.value),
        setValue => {
          var tracker = NoOpDisposableTracker.instance;
          var a1Sub = a1Src.subscribeWithoutEmit(tracker, a1 => setValue(zipper(a1, a2Src.value, a3Src.value, a4Src.value)));
          var a2Sub = a2Src.subscribeWithoutEmit(tracker, a2 => setValue(zipper(a1Src.value, a2, a3Src.value, a4Src.value)));
          var a3Sub = a3Src.subscribeWithoutEmit(tracker, a3 => setValue(zipper(a1Src.value, a2Src.value, a3, a4Src.value)));
          var a4Sub = a4Src.subscribeWithoutEmit(tracker, a4 => setValue(zipper(a1Src.value, a2Src.value, a3Src.value, a4)));
          return a1Sub.join(a2Sub, a3Sub, a4Sub);
        }
      );

    public static IRxVal<R> zip<A1, A2, A3, A4, A5, R>(
      this IRxVal<A1> a1Src, IRxVal<A2> a2Src, IRxVal<A3> a3Src, IRxVal<A4> a4Src, IRxVal<A5> a5Src,
      Func<A1, A2, A3, A4, A5, R> zipper
    ) =>
      new RxVal<R>(
        zipper(a1Src.value, a2Src.value, a3Src.value, a4Src.value, a5Src.value),
        setValue => {
          var tracker = NoOpDisposableTracker.instance;
          var a1Sub = a1Src.subscribeWithoutEmit(tracker, a1 => setValue(zipper(a1, a2Src.value, a3Src.value, a4Src.value, a5Src.value)));
          var a2Sub = a2Src.subscribeWithoutEmit(tracker, a2 => setValue(zipper(a1Src.value, a2, a3Src.value, a4Src.value, a5Src.value)));
          var a3Sub = a3Src.subscribeWithoutEmit(tracker, a3 => setValue(zipper(a1Src.value, a2Src.value, a3, a4Src.value, a5Src.value)));
          var a4Sub = a4Src.subscribeWithoutEmit(tracker, a4 => setValue(zipper(a1Src.value, a2Src.value, a3Src.value, a4, a5Src.value)));
          var a5Sub = a5Src.subscribeWithoutEmit(tracker, a5 => setValue(zipper(a1Src.value, a2Src.value, a3Src.value, a4Src.value, a5)));
          return a1Sub.join(a2Sub, a3Sub, a4Sub, a5Sub);
        }
      );

    #endregion

    // TODO: test
    /** Convert an enum of rx values into one rx value using a traversal function. **/
    public static IRxVal<B> traverse<A, B>(
      this IEnumerable<IRxVal<A>> valsEnum, Func<IEnumerable<A>, B> traverseFn
    ) => traverse(valsEnum.ToArray(), traverseFn);

    /// <summary>
    /// Convert an enum of rx values into one rx value using a traversal function.
    /// </summary>
    public static IRxVal<B> traverse<A, B>(
      this ICollection<IRxVal<A>> vals, Func<IEnumerable<A>, B> traverseFn
    ) {
      B getValue() => traverseFn(vals.Select(v => v.value));

      return new RxVal<B>(
        getValue(),
        setValue =>
          vals
            .Select(v => v.subscribeWithoutEmit(NoOpDisposableTracker.instance, _ => setValue(getValue())))
            .ToArray() // strict evaluation
            .joinSubscriptions()
      );
    }

    /// <summary>
    /// Returns any value that satisfies the predicate. Order is not guaranteed.
    /// </summary>
    public static IRxVal<Functional.Option<A>> anyThat<A, Coll>(
      this Coll vals, Func<A, bool> predicate
    ) where Coll : IEnumerable<IRxVal<A>> {
      var dict = new Dictionary<IRxVal<A>, A>();

      var lastKnownValue = F.none<A>();
      var rxVal = new RxVal<Functional.Option<A>>(
        lastKnownValue,
        setValue => {
          void set(Functional.Option<A> value) {
            lastKnownValue = value;
            setValue(value);
          }

          var subscriptions = vals.Select(rx => rx.subscribe(
            NoOpDisposableTracker.instance,
            a => {
              var matched = predicate(a);

              if (matched) {
                dict[rx] = a;
                if (lastKnownValue.isNone)
                  set(a.some());
              }
              else {
                dict.Remove(rx);
                if (lastKnownValue.isSome) {
                  set(dict.isEmpty() ? Functional.Option<A>.None : dict.First().Value.some());
                }
              }
            }
          )).ToArray(); // ToArray is required to delazify Select
          return subscriptions.joinSubscriptions();
        }
      );

      return rxVal;
    }

    public static IRxVal<Functional.Option<A>> anyThat<A>(
      this IEnumerable<IRxVal<A>> vals, Func<A, bool> predicate
    ) => vals.anyThat<A, IEnumerable<IRxVal<A>>>(predicate);

    public static IRxVal<bool> anyOf<C>(this C vals, bool searchFor=true)
      where C : IEnumerable<IRxVal<bool>>
    =>
      vals.anyThat<bool, C>(b => searchFor ? b : !b).map(_ => _.isSome);

    public static IRxVal<Functional.Option<A>> anyDefined<A>(
      this IEnumerable<IRxVal<Functional.Option<A>>> vals
    ) =>
      vals
      .anyThat<Functional.Option<A>, IEnumerable<IRxVal<Functional.Option<A>>>>(opt => opt.isSome)
      .map(_ => _.flatten());

    // TODO: test
    /// <summary>
    /// Convert <see cref="IRxVal{A}"/> to <see cref="IRxObservable"/>.
    ///
    /// Useful for converting from <see cref="IRxVal{A}"/> to event source. For example:
    ///
    /// <code><![CDATA[
    ///   someRxVal.map(_ => F.unit)
    /// ]]></code>
    ///
    /// would only emit one event, because the result of a map would be a <see cref="IRxVal{A}"/>
    /// that has a <see cref="Unit"/> type, which by it's definition only has one value.
    ///
    /// Thus we'd need to use
    /// <code><![CDATA[
    ///   someRxVal.toEventSource(_ => F.unit)
    /// ]]></code>
    /// </summary>
    public static IRxObservable<B> toEventSource<A, B>(
      this IRxVal<A> rxVal, Func<A, B> mapper
    ) => new Observable<B>(onEvent =>
      rxVal.subscribe(NoOpDisposableTracker.instance, v => onEvent(mapper(v)))
    );

    public static IRxObservable<Unit> toEventSource<A>(this IRxVal<A> o) =>
      o.toEventSource(_ => F.unit);

    public static IRxVal<Functional.Option<B>> optFlatMap<A, B>(
      this IRxVal<Functional.Option<A>> source, Func<A, IRxVal<Functional.Option<B>>> extractor
    ) =>
      source.flatMap(aOpt =>
        aOpt.fold(
          () => RxVal.cached(F.none<B>()),
          extractor
        )
      );

    public static IRxVal<Functional.Option<B>> optFlatMap<A, B>(
      this IRxVal<Functional.Option<A>> source, Func<A, IRxVal<B>> extractor
    ) =>
      source.optFlatMap(a => extractor(a).map(b => b.some()));

    public static IRxVal<Functional.Option<B>> optFlatMap<A, B>(
      this IRxVal<Functional.Option<A>> source, Func<A, Functional.Option<IRxVal<Functional.Option<B>>>> extractor
    ) =>
      source.flatMap(aOpt =>
        aOpt.flatMap(extractor).getOrElse(RxVal.cached(F.none<B>()))
      );

    public static IRxVal<Functional.Option<B>> optMap<A, B>(
      this IRxVal<Functional.Option<A>> source, Func<A, B> mapper
    ) => source.map(aOpt => aOpt.map(mapper));

    public static IRxVal<Functional.Option<A>> extract<A>(this Functional.Option<IRxVal<A>> rxOpt) =>
      rxOpt.fold(RxVal.cached(F.none<A>()), val => val.map(a => a.some()));

    public static Func<A, A> filterMapper<A>(Func<A, bool> predicate, Func<A> onFiltered) =>
      a => predicate(a) ? a : onFiltered();

    public static Func<A, A> filterMapper<A>(Func<A, bool> predicate, A onFiltered) =>
      a => predicate(a) ? a : onFiltered;
  }
}