using System;
using System.Runtime.CompilerServices;
using pzd.lib.collection;
using pzd.lib.data.dispose;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubject : IRxObservable {}
  public interface ISubject<A> : ISubject, IRxObservable<A>, IObserver<A> {}

  /// <summary>
  /// A subject is something that is <see cref="Observable{A}"/> and <see cref="IObserver{A}"/>
  /// at the same time.
  /// </summary>
  public class Subject<A> : Observable<A>, ISubject<A> {
    public void push(A value) => submit(value);
  }

  /// <summary>
  /// Replay subject stores all the events that comes into it and resubmits them upon subscription.
  /// </summary>
  public class ReplaySubject<A> : Observable<A>, ISubject<A> {
    A[] events = EmptyArray<A>._;
    uint eventsCount;

    public override void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // ReSharper disable ExplicitCallerInfoArgument
      base.subscribe(tracker, onEvent, out subscription, callerMemberName, callerFilePath, callerLineNumber);
      // ReSharper restore ExplicitCallerInfoArgument
      for (var idx = 0u; idx < eventsCount; idx++) onEvent(events[idx]);
    }

    public void push(A value) {
      submit(value);
      AList.add(ref events, ref eventsCount, value);
    }

    /// <summary>Clears the event backlog.</summary>
    public void clear() => AList.clear(events, ref eventsCount);
  }

  /// <summary>
  /// Cache subject stores events when there are 0 subscribers, and pushes them all at once on new subscription
  /// </summary>
  public class CacheSubject<A> : Observable<A>, ISubject<A> {
    A[] cache = EmptyArray<A>._;
    uint cacheCount;

    public override void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // ReSharper disable ExplicitCallerInfoArgument
      base.subscribe(tracker, onEvent, out subscription, callerMemberName, callerFilePath, callerLineNumber);
      // ReSharper restore ExplicitCallerInfoArgument
      for (var idx = 0u; idx < cacheCount; idx++) onEvent(cache[idx]);
      AList.clear(cache, ref cacheCount);
    }

    public void push(A value) {
      if (subscribers == 0) {
        AList.add(ref cache, ref cacheCount, value);
      }
      else {
        submit(value);
      }
    }
  }
}
