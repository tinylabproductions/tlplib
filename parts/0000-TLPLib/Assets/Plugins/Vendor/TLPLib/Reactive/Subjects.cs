using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Reactive {
  public interface ISubject : IObservable {}
  public interface ISubject<A> : ISubject, IObservable<A>, IObserver<A> {}

  /** 
   * A subject is something that is Observable and Observer at the same
   * time.
   **/
  public class Subject<A> : Observable<A>, ISubject<A> {
    public void push(A value) => submit(value);
  }

  /**
   * Replay subject stores all the events that comes into it and resubmits 
   * them upon subscription.
   **/
  public class ReplaySubject<A> : Observable<A>, ISubject<A> {
    // Some(value) - submited;
    readonly List<A> events = new List<A>();

    public override ISubscription subscribe(IObserver<A> observer) {
      foreach (var evt in events) observer.push(evt);
      return base.subscribe(observer);
    }

    public void push(A value) {
      submit(value);
      events.Add(value);
    }

    /** Clears the event backlog. */
    public void clear() => events.Clear();
  }
}
