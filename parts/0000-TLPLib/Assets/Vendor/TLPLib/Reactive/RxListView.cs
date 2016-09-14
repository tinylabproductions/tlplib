using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxListView {
    public static RxListView<A> a<A>(
      IRxList<A> list, int startIndex, int windowSize
    ) { return new RxListView<A>(list, startIndex, windowSize); }

    public static RxListView<A> view<A>(
      this IRxList<A> list, int startIndex, int windowSize
    ) { return a(list, startIndex, windowSize); }
  }

  /**
   * A windowed view into a RxList.
   * 
   * Allows you to have a RX list and have a window into it. 
   * 
   * For example: you have 100 inventory slots and only want to show
   * 10 on the screen at the same time.
   **/
  public sealed class RxListView<A> 
  : Observable<ReadOnlyCollection<Option<A>>>, 
    IRxVal<ReadOnlyCollection<Option<A>>> 
  {
    public readonly int windowSize;

    private readonly IRxList<A> list;
    private readonly IList<ISubscription> subscriptions = 
      new List<ISubscription>();
    private readonly List<Option<A>> viewValues;

    public ReadOnlyCollection<Option<A>> value 
      { get { return viewValues.AsReadOnly(); } }

    private int _startIndex = -1;

    public int startIndex {
      get { return _startIndex; }
      set {
        if (_startIndex == value) return;
        _startIndex = value;

        foreach (var s in subscriptions) s.unsubscribe();
        subscriptions.Clear();

        var start = _startIndex;
        var end = start + windowSize;
        for (var current = start; current < end; current++) {
          var i = current - start;
          subscriptions.Add(list.rxElement(current).subscribe(vOpt => {
            viewValues[i] = vOpt;
            submit();
          }));
          viewValues[i] = list.get(current);
        }
        submit();
      }
    }

    public RxListView(IRxList<A> list, int startIndex, int windowSize) {
      this.list = list;
      this.windowSize = windowSize;
      viewValues = new List<Option<A>>(windowSize);
      for (var i = 0; i < windowSize; i++) viewValues.Add(F.none<A>());

      this.startIndex = startIndex;
    }

    public int mapIndex(int viewIndex) {
      return startIndex + viewIndex;
    }

    public new IRxVal<B> map<B>(Fn<ReadOnlyCollection<Option<A>>, B> mapper) {
      return mapImpl(mapper, RxVal.builder(() => mapper(value)));
    }

    public IRxVal<B> flatMap<B>(Fn<ReadOnlyCollection<Option<A>>, IRxVal<B>> mapper) {
      return flatMapImpl(mapper, RxVal.builder(() => mapper(value).value));
    }

    public IRxVal<ReadOnlyCollection<Option<A>>> filter(
      Fn<ReadOnlyCollection<Option<A>>, bool> predicate, 
      Fn<ReadOnlyCollection<Option<A>>> onFilter
    ) { return map(RxVal.filterMapper(predicate, onFilter)); }

    public IRxVal<ReadOnlyCollection<Option<A>>> filter(
      Fn<ReadOnlyCollection<Option<A>>, bool> predicate, 
      ReadOnlyCollection<Option<A>> onFilter
    ) { return map(RxVal.filterMapper(predicate, onFilter)); }

    public IRxVal<Tpl<ReadOnlyCollection<Option<A>>, B>> zip<B>(IRxVal<B> ref2) 
    { return zipImpl(ref2, RxVal.builder(() => F.t(value, ref2.value))); }

    public IRxVal<Tpl<ReadOnlyCollection<Option<A>>, B, C>> zip<B, C>(IRxVal<B> ref2, IRxVal<C> ref3) 
    { return zipImpl(ref2, ref3, RxVal.builder(() => F.t(value, ref2.value, ref3.value))); }

    public IRxVal<Tpl<ReadOnlyCollection<Option<A>>, B, C, D>> zip<B, C, D>(
      IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4
    ) { return zipImpl(ref2, ref3, ref4, RxVal.builder(() => F.t(value, ref2.value, ref3.value, ref4.value))); }

    public IRxVal<Tpl<ReadOnlyCollection<Option<A>>, B, C, D, E>> zip<B, C, D, E>(
      IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5
    ) { return zipImpl(
      ref2, ref3, ref4, ref5,
      RxVal.builder(() => F.t(value, ref2.value, ref3.value, ref4.value, ref5.value))
    ); }

    public IRxVal<Tpl<ReadOnlyCollection<Option<A>>, A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      IRxVal<A1> ref2, IRxVal<A2> ref3, IRxVal<A3> ref4, IRxVal<A4> ref5, IRxVal<A5> ref6
    ) { return zipImpl(
      ref2, ref3, ref4, ref5, ref6,
      RxVal.builder(() => F.t(value, ref2.value, ref3.value, ref4.value, ref5.value, ref6.value))
    ); }

    private void submit() { submit(value); }
  }
}