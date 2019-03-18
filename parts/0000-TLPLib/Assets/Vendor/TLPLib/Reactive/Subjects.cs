using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using com.tinylabproductions.TLPLib.dispose;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubject : IRxObservable {}
  public interface ISubject<A> : ISubject, IRxObservable<A>, IRxObserver<A> {}

  /// <summary>
  /// A subject is something that is <see cref="Observable{A}"/> and <see cref="IRxObserver{A}"/>
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

    public override void subscribe(
      IDisposableTracker tracker, Act<A> onEvent, out ISubscription subscription,
      [CallerMemberName] string callerMemberName = "",
      [CallerFilePath] string callerFilePath = "",
      [CallerLineNumber] int callerLineNumber = 0
    ) {
      // ReSharper disable ExplicitCallerInfoArgument
      base.subscribe(tracker, onEvent, out subscription, callerMemberName, callerFilePath, callerLineNumber);
      // ReSharper restore ExplicitCallerInfoArgument
      foreach (var evt in events) onEvent(evt);
    }

    public void push(A value) {
      submit(value);
      events.Add(value);
    }

    /// <summary>Clears the event backlog.</summary>
    public void clear() => events.Clear();
  }
}
