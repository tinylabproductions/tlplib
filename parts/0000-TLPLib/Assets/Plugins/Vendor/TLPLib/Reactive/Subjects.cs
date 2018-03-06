using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.dispose;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubject : IObservable {}
  public interface ISubject<A> : ISubject, IObservable<A>, IObserver<A> {}

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
    readonly List<A> events = new List<A>();

    public override ISubscription subscribe(
      IDisposableTracker tracker, Act<A> onEvent,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // ReSharper disable ExplicitCallerInfoArgument
      var subscription = base.subscribe(tracker, onEvent, callerMemberName, callerFilePath, callerLineNumber);
      // ReSharper restore ExplicitCallerInfoArgument
      foreach (var evt in events) onEvent(evt);
      return subscription;
    }

    public void push(A value) {
      submit(value);
      events.Add(value);
    }

    /// <summary>Clears the event backlog.</summary>
    public void clear() => events.Clear();
  }
}
