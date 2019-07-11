using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  /// <summary>
  /// RxRef is a reactive reference, which stores a value and also acts as a IObservable.
  /// </summary>
  public interface IRxRef<A> : Ref<A>, IRxVal<A> {
    new A value { get; set; }
  }

  /// <summary>
  /// Mutable reference which is also an observable.
  /// </summary>
  public class RxRef<A> : Observable<A>, IRxRef<A> {
    readonly IEqualityComparer<A> comparer;

    A _value;
    public A value {
      get => _value;
      set {
        if (RxBase.compareAndSet(comparer, ref _value, value))
          submit(value);
      }
    }

    public RxRef(A value, IEqualityComparer<A> comparer = null) {
      this.comparer = comparer ?? EqComparer<A>.Default;
      _value = value;
    }

    public override void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      base.subscribe(
        tracker, onEvent, out subscription,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
      onEvent(value);
    }

    public ISubscription subscribeWithoutEmit(
      IDisposableTracker tracker, Action<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      base.subscribe(
        tracker, onEvent, out var sub,
        // ReSharper disable ExplicitCallerInfoArgument
        callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber
        // ReSharper restore ExplicitCallerInfoArgument
      );
      return sub;
    }

    public override string ToString() => $"{nameof(RxRef)}({value})";
  }

  public static class RxRef {
    public static IRxRef<A> a<A>(A value, IEqualityComparer<A> comparer = null) => 
      new RxRef<A>(value, comparer);
  }
}