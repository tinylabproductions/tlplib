using System;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxValOps {
    public static IRxVal<B> map<A, B>(this IRxVal<A> rx, Fn<A, B> mapper) =>
      RxVal.a(rx, mapper, ObservableOpImpls.map(rx, mapper));

    #region #filter

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Fn<A, bool> predicate, Fn<A> onFiltered) =>
      rx.map(RxVal.filterMapper(predicate, onFiltered));

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Fn<A, bool> predicate, A onFiltered) =>
      rx.map(RxVal.filterMapper(predicate, onFiltered));

    #endregion

    public static IRxVal<B> flatMap<A, B>(this IRxVal<A> rx, Fn<A, IRxVal<B>> mapper) {
      var lastAVersion = Option<uint>.None;
      var lastBVersion = Option<uint>.None;
      var lastBRx = Option<IRxVal<B>>.None;
      var sp = new RxVal<B>.SourceProperties(
        () => 
          lastAVersion.isEmpty || lastAVersion.get != rx.valueVersion
          || lastBRx.exists(bRx => bRx)
          ,
        () => {
          var newRx = mapper(rx.value);
          lastVersion = newRx.valueVersion.some();
          return newRx.value;
        }
      );

      RxVal.a(
        sp,
        ObservableOpImpls.flatMap<A, B, IRxVal<B>>(rx, mapper, newRx => {})
      );
    }

    #region #zip

    public static IRxVal<Tpl<A, B>> zip<A, B>(this IRxVal<A> rx, IRxVal<B> rx2) =>
      RxVal.a(
        () => F.t(rx.value, rx2.value),
        ObservableOpImpls.zip(rx, rx2)
      );

    public static IRxVal<Tpl<A, B, C>> zip<A, B, C>(
      this IRxVal<A> rx, IRxVal<B> rx2, IRxVal<C> rx3
    ) => RxVal.a(
      () => F.t(rx.value, rx2.value, rx3.value),
      ObservableOpImpls.zip(rx, rx2, rx3)
    );

    public static IRxVal<Tpl<A, B, C, D>> zip<A, B, C, D>(
      this IRxVal<A> ref1, IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4
    ) => RxVal.a(
      () => F.t(ref1.value, ref2.value, ref3.value, ref4.value),
      ObservableOpImpls.zip(ref1, ref2, ref3, ref4)
    );

    public static IRxVal<Tpl<A, B, C, D, E>> zip<A, B, C, D, E>(
      this IRxVal<A> ref1, IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5
    ) => RxVal.a(
      () => F.t(ref1.value, ref2.value, ref3.value, ref4.value, ref5.value),
      ObservableOpImpls.zip(ref1, ref2, ref3, ref4, ref5)
    );

    public static IRxVal<Tpl<A, A1, A2, A3, A4, A5>> zip<A, A1, A2, A3, A4, A5>(
      this IRxVal<A> ref1, IRxVal<A1> ref2, IRxVal<A2> ref3, IRxVal<A3> ref4, IRxVal<A4> ref5, 
      IRxVal<A5> ref6
    ) => RxVal.a(
      () => F.t(ref1.value, ref2.value, ref3.value, ref4.value, ref5.value, ref6.value),
      ObservableOpImpls.zip(ref1, ref2, ref3, ref4, ref5, ref6)
    );

    #endregion
  }
}