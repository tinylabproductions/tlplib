using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class IObservableTestExts {
    public static void testUnsubAndFinish<A, B>(
      ISubscription sub, IObservable<A> obs, params Subject<B>[] subjects
    ) {
      sub.testUnsubscriptionC(subjects);
      obs.testFinishing(subjects);
    }

    public static void testFinishing<A, B>(
      this Subject<A> subj, Fn<Subject<A>, IObservable<B>> obsFn
    ) => obsFn(subj).testFinishing(subj);

    public static void testFinishing<A, B>(
      this IObservable<A> obs, params Subject<B>[] subjects
    ) {
      var sub = obs.subscribe(_ => { });
      sub.isSubscribed.shouldBeTrue("Subscription should be active before finish.");
      obs.finished.shouldBeFalse("Observable should not be finished prior to source finish.");
      for (var idx = 0; idx < subjects.Length - 1; idx++) {
        subjects[idx].finish();
        var cond = $"when {idx + 1}/{subjects.Length} sources  has finished";
        sub.isSubscribed.shouldBeTrue($"Subscription should be active {cond}.");
        obs.finished.shouldBeFalse($"Observable should not be finished {cond}.");
      }
      subjects.Last().finish();
      obs.finished.shouldBeTrue("Observable should be finished after all sources finish.");
      sub.isSubscribed.shouldBeFalse("Subscription should be cancelled after all sources finish.");
    }

    public static void testUnsubscription<A>(
      this ISubscription subscription, params IObservable<A>[] observables
    ) => testUnsubscriptionC(subscription, observables);

    public static void testUnsubscriptionC<A>(
      this ISubscription subscription, ICollection<IObservable<A>> observables
    ) {
      subscription.isSubscribed.shouldBeTrue("it should be subscribed to aggregate subscription");
      foreach (var obs in observables)
        obs.subscribers.shouldEqual(1, "aggregate should be subscribed to sources");
      subscription.unsubscribe();
      subscription.isSubscribed.shouldBeFalse("aggregate should unsubscribe");
      foreach (var obs in observables)
        obs.subscribers.shouldEqual(0, "aggregate should be unsubscribe from sources");
    }

    public static Tpl<List<A>, ISubscription> pipeToList<A>(this IObservable<A> obs) {
      var list = new List<A>();
      var sub = obs.subscribe(list.Add);
      return F.t(list, sub);
    }

    public static Tpl<Ref<uint>, ISubscription> countEvents<A>(this IObservable<A> obs) {
      var r = Ref.a(0u);
      var sub = obs.subscribe(_ => r.value++);
      return F.t(r, sub);
    }

    public static Tpl<Ref<Option<A>>, ISubscription> pipeToRef<A>(this IObservable<A> obs) {
      var reference = Ref.a(Option<A>.None);
      var sub = obs.subscribe(a => reference.value = a.some());
      return F.t(reference, sub);
    }

    public static void observableCreateListQueue<A>(Act<IObservableQueue<A, List<A>>> act) {
      var c = new List<A>();
      var queue = Observable.createQueue(
        a => c.Add(a), () => c.RemoveAt(0), () => c.Count, () => c,
        c.First, c.Last
      );
      act(queue);
    }
  }
}