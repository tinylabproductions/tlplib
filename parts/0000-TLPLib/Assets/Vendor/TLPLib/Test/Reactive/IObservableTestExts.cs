using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using pzd.lib.data.dispose;
using pzd.lib.functional;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class IObservableTestExts {
    public static void testUnsubAndFinish(
      ISubscription sub, params ISubject[] subjects
    ) {
      sub.testUnsubscriptionC(subjects);
    }

    public static void testUnsubscription(
      this ISubscription subscription, params IRxObservable[] observables
    ) => testUnsubscriptionC(subscription, observables);

    public static void testUnsubscriptionC(
      this ISubscription subscription, ICollection<IRxObservable> observables
    ) {
      subscription.isSubscribed.shouldBeTrue("it should be subscribed to aggregate subscription");
      foreach (var obs in observables)
        obs.subscribers.shouldEqual(1, "aggregate should be subscribed to sources");
      subscription.unsubscribe();
      subscription.isSubscribed.shouldBeFalse("aggregate should unsubscribe");
      foreach (var obs in observables)
        obs.subscribers.shouldEqual(0, "aggregate should be unsubscribe from sources");
    }

    public static Tpl<List<A>, ISubscription> pipeToList<A>(
      this IRxObservable<A> obs, IDisposableTracker tracker
    ) {
      var list = new List<A>();
      var sub = obs.subscribe(tracker, list.Add);
      return F.t(list, sub);
    }

    public static Tpl<Ref<uint>, ISubscription> countEvents<A>(
      this IRxObservable<A> obs, IDisposableTracker tracker
    ) {
      var r = Ref.a(0u);
      var sub = obs.subscribe(tracker, _ => r.value++);
      return F.t(r, sub);
    }

    public static Tpl<Ref<Option<A>>, ISubscription> pipeToRef<A>(
      this IRxObservable<A> obs, IDisposableTracker tracker
    ) {
      var reference = Ref.a(Option<A>.None);
      var sub = obs.subscribe(tracker, a => reference.value = a.some());
      return F.t(reference, sub);
    }

    public static void observableCreateListQueue<A>(Action<IObservableQueue<A, List<A>>> act) {
      var c = new List<A>();
      var queue = Observable.createQueue(
        a => c.Add(a), () => c.RemoveAt(0), () => c.Count, () => c,
        c.First, c.Last
      );
      act(queue);
    }
  }
}