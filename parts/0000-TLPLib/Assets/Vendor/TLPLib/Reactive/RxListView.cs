using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxListView {
    public static RxListView<A> a<A>(
      RxList<A> list, int startIndex, int windowSize
    ) { return new RxListView<A>(list, startIndex, windowSize); }
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

    private readonly RxList<A> list;
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

    public RxListView(RxList<A> list, int startIndex, int windowSize) {
      this.list = list;
      this.windowSize = windowSize;
      viewValues = new List<Option<A>>(windowSize);
      for (var i = 0; i < windowSize; i++) viewValues.Add(F.none<A>());

      this.startIndex = startIndex;
    }

    public override ISubscription subscribe(Act<ReadOnlyCollection<Option<A>>> onChange)
      { return subscribe(onChange, emitCurrent: true); }

    public ISubscription subscribe(
      Act<ReadOnlyCollection<Option<A>>> onChange, bool emitCurrent
    ) {
      var subscription = base.subscribe(onChange);
      if (emitCurrent) onChange(value); // Emit current value on subscription.
      return subscription;
    }

    public int mapIndex(int viewIndex) {
      return startIndex + viewIndex;
    }

    public new IRxVal<B> map<B>(Fn<ReadOnlyCollection<Option<A>>, B> mapper) {
      return mapImpl(mapper, RxVal.builder(mapper(value)));
    }

    private void submit() { submit(value); }
  }
}
