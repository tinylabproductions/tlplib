using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * IValueObservable is an observable which has a current value.
   **/
  public interface IValueObservable<A> : IObservable<A> {
    A value { get; }
    ISubscription subscribe(Act<A> onChange, bool emitCurrent);
  }

  /**
   * RxVal is IValueObservable that can do some other operations.
   **/
  public interface IRxVal<A> : IValueObservable<A> {
    new IRxVal<B> map<B>(Fn<A, B> mapper);
  }
  
  public static class RxVal {
    public static ObserverBuilder<Elem, IRxVal<Elem>> builder<Elem>(Elem value) {
      return RxRef.builder(value);
    }

    /** Convert an enum of rx values into one rx value using a traversal function. **/
    public static IRxVal<B> traverse<A, B>(
      this IEnumerable<IRxVal<A>> vals, Fn<IEnumerable<A>, B> traverse
    ) {
      Fn<IEnumerable<A>> readValues = () => vals.Select(v => v.value);
      var val = RxRef.a(traverse(readValues()));

      // TODO: this is probably suboptimal.
      Act rescan = () => val.value = traverse(readValues());

      foreach (var rxVal in vals) rxVal.subscribe(_ => rescan(), emitCurrent:false);
      rescan();

      return val;
    }

    /* Returns first value that satisfies the predicate. */
    public static IRxVal<Option<A>> firstThat<A>(this IEnumerable<IRxVal<A>> vals, Fn<A, bool> predicate) {
      var val = RxRef.a(F.none<A>());

      // TODO: this is probably suboptimal.
      Act rescan = () => {
        foreach (var rxVal in vals.Where(rxVal => predicate(rxVal.value))) {
          val.value = F.some(rxVal.value);
          return;
        }
        val.value = F.none<A>();
      };

      foreach (var rxVal in vals) rxVal.subscribe(_ => rescan(), emitCurrent:false);
      rescan();

      return val;
    }

    public static IRxVal<bool> anyOf(this IEnumerable<IRxVal<bool>> vals, bool searchForTrue=true) 
      { return vals.firstThat(b => searchForTrue ? b : !b).map(_ => _.isDefined); }

    public static IRxVal<A> extractFuture<A>(
      this Future<IRxVal<A>> future, A whileNotCompleted
    ) {
      var rx = RxRef.a(whileNotCompleted);
      future.onSuccess(rx2 => rx2.subscribe(v => rx.value = v));
      return rx;
    }
  }
}
