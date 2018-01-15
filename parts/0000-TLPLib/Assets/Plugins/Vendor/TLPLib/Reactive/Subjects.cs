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

  /// <summary>
  /// Replay subject stores all the events that comes into it and resubmits them upon subscription.
  /// </summary>
  public class ReplaySubject<A> : Observable<A>, ISubject<A> {
    // Some(value) - submited;
    readonly List<A> events = new List<A>();

    public override ISubscription subscribe(IObserver<A> observer) {
<<<<<<< HEAD
      foreach (var evt in events) observer.push(evt);
=======
      foreach (var opt in events) {
        if (opt.isSome) observer.push(opt.get);
        else observer.finish();
      }
>>>>>>> master
      return base.subscribe(observer);
    }

    public void push(A value) {
      submit(value);
      events.Add(value);
    }

    /** Clears the event backlog. */
    public void clear() => events.Clear();
  }

  /// <summary>Subject that only allows having one subscription.</summary>
  public class SingleSubscriberSubject<A> : Observable<A>, ISubject<A> {
    ISubscription lastSubscription = Subscription.empty;

    public override ISubscription subscribe(IObserver<A> observer) {
      lastSubscription.unsubscribe();
      lastSubscription = base.subscribe(observer);
      return lastSubscription;
    }

    public void push(A value) => submit(value);
    public void finish() => finishObservable();
  }
}
