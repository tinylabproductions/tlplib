using System.Collections.Generic;
using System.Collections.ObjectModel;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxListView {
    public static RxListView<A> a<A>(
      IRxList<A> list, int startIndex, int windowSize
    ) => new RxListView<A>(list, startIndex, windowSize);

    public static RxListView<A> view<A>(
      this IRxList<A> list, int startIndex, int windowSize
    ) => a(list, startIndex, windowSize);
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

    readonly IRxList<A> list;
    readonly IList<ISubscription> subscriptions = 
      new List<ISubscription>();
    readonly List<Option<A>> viewValues;

    public ReadOnlyCollection<Option<A>> value => viewValues.AsReadOnly();
    public ReadOnlyCollection<Option<A>> currentValue => value;

    int _startIndex = -1;

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

    public int mapIndex(int viewIndex) => startIndex + viewIndex;

    void submit() => submit(value);
  }
}