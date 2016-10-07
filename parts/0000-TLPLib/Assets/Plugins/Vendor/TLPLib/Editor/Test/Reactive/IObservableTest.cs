using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class IObservableTestSubscriptionCounting {
    [Test]
    public void Simple() {
      var o = new Subject<Unit>();
      var s1 = o.subscribe(_ => {});
      var s2 = o.subscribe(_ => {});
      o.subscribers.shouldEqual(2);
      s1.unsubscribe();
      o.subscribers.shouldEqual(1);
      s1.unsubscribe();
      o.subscribers.shouldEqual(1);
      s2.unsubscribe();
      o.subscribers.shouldEqual(0);
      var s3 = o.subscribe(_ => { });
      o.subscribers.shouldEqual(1);
      s3.unsubscribe();
      o.subscribers.shouldEqual(0);
    }

    [Test]
    public void Nested() {
      var o = new Subject<Unit>();
      var o2 = o.map(_ => 1);
      Assert.AreEqual(0, o.subscribers);
      Assert.AreEqual(0, o2.subscribers);
      var s = o2.subscribe(_ => { });
      Assert.AreEqual(1, o.subscribers);
      Assert.AreEqual(1, o2.subscribers);
      s.unsubscribe();
      Assert.AreEqual(0, o.subscribers);
      Assert.AreEqual(0, o2.subscribers);
    }
  }

  public class IObservableSubscriptionTest {
    [Test]
    public void SubscribeFromInsideEvent() {
      var subject = new Subject<Unit>();
      var pushedOuter = 0;
      var pushedInner = 0;
      subject.subscribe(_ => {
        pushedOuter++;
        subject.subscribe(__ => pushedInner++);
        subject.subscribers.shouldEqual(
          pushedOuter, 
          "it wait until event dispatching is completed until subscribing the observable"
        );
      });
      subject.push(F.unit);
      subject.subscribers.shouldEqual(2, "it should subscribe after the event has been dispatched");
      pushedOuter.shouldEqual(1, "it should get the simple event");
      pushedInner.shouldEqual(0, "it should not get the event which caused subscribe to happen");

      subject.push(F.unit);
      subject.subscribers.shouldEqual(3, "inner subscription should work for subsequent events");
      pushedOuter.shouldEqual(2, "it should work for simple events");
      pushedInner.shouldEqual(1, "it should not get the event which caused subscribe to happen");
    }

    [Test]
    public void PushFromInsideEvent() {
      var subj = new Subject<int>();
      var list = F.emptyList<Tpl<int, char>>();
      subj.subscribe(a => {
        list.Add(F.t(a, 'a'));
        if (a == 0) {
          subj.push(1);
          subj.push(2);
        }
      });
      subj.subscribe(a => {
        list.Add(F.t(a, 'b'));
      });
      list.shouldBeEmpty();
      subj.push(0);
      list.shouldEqual(F.list(
        F.t(0, 'a'), F.t(0, 'b'),
        F.t(1, 'a'), F.t(1, 'b'),
        F.t(2, 'a'), F.t(2, 'b')
      ));
    }

    [Test]
    public void UnsubscribeAfterEvent() {
      var subject = new Subject<Unit>();
      var called = 0;
      subject.subscribe((_, subscription) => {
        called += 1;
        subscription.unsubscribe();
      });
      subject.push(F.unit);
      subject.push(F.unit);
      Assert.AreEqual(1, called);
    }
  }

  public class IObservableTestToFuture {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var f = subj.toFuture();
      f.value.shouldBeNone();
      subj.push(1);
      f.value.shouldBeSome(1);
      subj.subscribers.shouldEqual(0, "it should unsubscribe after completing the future");
    }
  }

  public class IObservableTestMap {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.map(i => i * 2);
      obs.pipeToList().ua((list, sub) => {
        subj.pushMany(1, 2, 3, 4, 5);
        list.shouldEqual(F.list(2, 4, 6, 8, 10));
        sub.testUnsubscription(subj);
        obs.testFinishing(subj);
      });
    }
  }

  public class IObservableTestDiscardValue {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.discardValue();
      obs.countEvents().ua((evts, sub) => {
        evts.value.shouldEqual(0u);
        subj.pushMany(1, 2, 3, 4);
        evts.value.shouldEqual(4u);
        sub.testUnsubscription(subj);
        obs.testFinishing(subj);
      });
    }
  }

  public class IObservableTestFlatMap {
    [Test]
    public void TestIEnumerable() {
      var subj = new Subject<int>();
      var obs = subj.flatMap(i => Enumerable.Range(0, i));
      obs.pipeToList().ua((list, sub) => {
        subj.pushMany(1, 2, 3);
        list.shouldEqual(F.list(
          0,
          0, 1,
          0, 1, 2
        ));
        sub.testUnsubscription(subj);
        obs.testFinishing(subj);
      });
    }

    [Test]
    public void TestIObservable() {
      var subj = new Subject<int>();
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj.flatMap(i => i % 2 == 0 ? subj1 : subj2);
      var t = obs.pipeToList();
      var list = t._1;
      var sub = t._2;
      Act<int, int> checkSubs = (s1, s2) => {
        subj1.subscribers.shouldEqual(s1);
        subj2.subscribers.shouldEqual(s2);
      };

      list.shouldBeEmpty();
      checkSubs(0, 0);

      subj.push(0);
      list.shouldBeEmpty();
      checkSubs(1, 0);

      subj1.push(1);
      subj2.push(-1);
      list.shouldEqual(F.list(1));

      subj.push(1);
      list.shouldEqual(F.list(1));
      checkSubs(0, 1);

      subj1.push(2);
      subj2.push(-2);
      list.shouldEqual(F.list(1, -2));

      subj.push(2);
      list.shouldEqual(F.list(1, -2));
      checkSubs(1, 0);

      subj1.push(3);
      subj2.push(-3);
      list.shouldEqual(F.list(1, -2, 3));

      sub.testUnsubscription(subj);
      obs.testFinishing(subj);
      checkSubs(0, 0);
    }

    [Test]
    public void TestFuture() {
      Promise<Unit> gateP;
      var gateF = Future<Unit>.async(out gateP);
      var subj = new Subject<int>();
      var obs = subj.flatMap(i => gateF.map(_ => i));
      var t = obs.pipeToList();
      var list = t._1;
      var sub = t._2;
      subj.pushMany(1, 2, 3);
      list.shouldBeEmpty();
      gateP.complete(F.unit);
      list.shouldEqual(F.list(1, 2, 3));
      sub.testUnsubscription(subj);
      obs.testFinishing(subj);
    }

    [Test]
    public void TestWhenFutureCompletesAfterFinish() {
      Promise<Unit> gateP;
      var gateF = Future<Unit>.async(out gateP);
      var subj = new Subject<int>();
      var obs = subj.flatMap(i => gateF.map(_ => i));
      var t = obs.pipeToList();
      var list = t._1;
      subj.pushMany(1, 2, 3);
      list.shouldBeEmpty();
      subj.finish();
      list.shouldBeEmpty();
      gateP.complete(F.unit);
      list.shouldBeEmpty();
    }
  }

  public class IObservableTestFilter {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.filter(i => i % 2 == 0);
      var t = obs.pipeToList();
      var list = t._1;
      var sub = t._2;
      foreach (var i in Enumerable.Range(0, 5)) subj.push(i);
      list.shouldEqual(F.list(0, 2, 4));
      sub.testUnsubscription(subj);
      obs.testFinishing(subj);
    }
  }

  public class IObservableTestCollect {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.collect(i => (i % 2 == 0).opt(i * 2));
      var t = obs.pipeToList();
      var list = t._1;
      var sub = t._2;
      subj.pushMany(0, 1, 2, 3, 4, 5, 6);
      list.shouldEqual(F.list(0, 4, 8, 12));
      sub.testUnsubscription(subj);
      obs.testFinishing(subj);
    }
  }

  public class IObservableTestReadOnlyLinkedListCollection {
    [Test]
    public void Test() {
      var c = new ObservableReadOnlyLinkedListQueue<int>();
      c.count.shouldEqual(0);
      c.collection.shouldBeEmpty();
      c.addLast(1);
      c.addLast(2);
      c.addLast(3);
      c.count.shouldEqual(3);
      c.collection.ToList().shouldEqual(F.list(1, 2, 3));
      c.removeFirst();
      c.count.shouldEqual(2);
      c.collection.ToList().shouldEqual(F.list(2, 3));
      c.removeFirst();
      c.count.shouldEqual(1);
      c.collection.ToList().shouldEqual(F.list(3));
      c.removeFirst();
      c.count.shouldEqual(0);
      c.collection.shouldBeEmpty();
    }
  }

  public class IObservableTestBuffer {
    [Test]
    public void Test() {
      test(
        subj => subj.buffer(3),
        args => new ReadOnlyLinkedList<int>(new LinkedList<int>(args))
      );
    }

    [Test]
    public void TestCustomCollection() {
      IObservableTestExts.observableCreateListQueue<int>(queue => test(
        subj => subj.buffer(3, queue),
        args => args
      ));
    }

    static void test<C>(
      Fn<Subject<int>, IObservable<C>> createObs, Fn<List<int>, C> createCollection
    ) where C : IEnumerable<int> {
      var subj = new Subject<int>();
      var obs = createObs(subj);
      var t = obs.pipeToRef();
      var r = t._1;
      var sub = t._2;
      r.value.shouldBeNone();
      subj.push(1);
      r.value.shouldBeSomeEnum(createCollection(F.list(1)));
      subj.push(2);
      r.value.shouldBeSomeEnum(createCollection(F.list(1, 2)));
      subj.push(3);
      r.value.shouldBeSomeEnum(createCollection(F.list(1, 2, 3)));
      subj.push(4);
      r.value.shouldBeSomeEnum(createCollection(F.list(2, 3, 4)));
      subj.push(5);
      r.value.shouldBeSomeEnum(createCollection(F.list(3, 4, 5)));

      sub.testUnsubscription(subj);
      obs.testFinishing(subj);
    }
  }

  // TODO: test timeBuffer with integration tests

  public class IObservableTestJoin {
    [Test]
    public void Test() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj1.join(subj2);
      obs.pipeToList().ua((list, sub) => {
        subj1.push(1);
        subj2.push(2);
        subj1.push(3);
        subj2.push(4);
        list.shouldEqual(F.list(1, 2, 3, 4));
        sub.testUnsubscription(subj1, subj2);
        obs.testFinishing(subj1, subj2);
      });
    }
  }

  public class IObservableTestJoinAll {
    [Test]
    public void Test() {
      var subjs = new[] {new Subject<int>(), new Subject<int>(), new Subject<int>()};
      var obs = subjs.joinAll();
      var t = obs.pipeToList();
      foreach (var subj in subjs) subj.push(1);
      t._1.shouldEqual(F.list(1, 1, 1));
      t._2.testUnsubscriptionC(subjs);
      obs.testFinishing(subjs);
    }
  }

  public class IObservableTestJoinDiscard {
    [Test]
    public void Test() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj1.joinDiscard(subj2);
      var t = obs.countEvents();
      var evts = t._1;
      subj1.pushMany(1, 2, 3);
      subj2.pushMany(1, 2, 3);
      evts.value.shouldEqual(6u);
      
      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj1, subj2);
    }
  }

  public class IObservableTestZip {
    [Test]
    public void Zip2() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj1.zip(subj2);
      var t = obs.pipeToRef();
      var r = t._1;
      subj1.push(1);
      r.value.shouldBeNone();
      subj2.push(1);
      r.value.shouldBeSome(F.t(1, 1));
      subj2.push(2);
      r.value.shouldBeSome(F.t(1, 2));
      subj1.push(0);
      r.value.shouldBeSome(F.t(0, 2));
      
      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj1, subj2);
    }

    [Test]
    public void Zip3() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3);
      var t = obs.pipeToRef();
      var r = t._1;
      subj1.push(1);
      r.value.shouldBeNone();
      subj2.push(1);
      r.value.shouldBeNone();
      subj3.push(1);
      r.value.shouldBeSome(F.t(1, 1, 1));
      subj2.push(2);
      r.value.shouldBeSome(F.t(1, 2, 1));
      subj3.push(0);
      r.value.shouldBeSome(F.t(1, 2, 0));
      subj1.push(3);
      r.value.shouldBeSome(F.t(3, 2, 0));
      
      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj1, subj2, subj3);
    }

    [Test]
    public void Zip4() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var subj4 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, subj4);
      var t = obs.pipeToRef();
      var r = t._1;
      subj1.push(1);
      r.value.shouldBeNone();
      subj2.push(1);
      r.value.shouldBeNone();
      subj3.push(1);
      r.value.shouldBeNone();
      subj4.push(1);
      r.value.shouldBeSome(F.t(1, 1, 1, 1));
      subj2.push(2);
      r.value.shouldBeSome(F.t(1, 2, 1, 1));
      subj3.push(0);
      r.value.shouldBeSome(F.t(1, 2, 0, 1));
      subj1.push(3);
      r.value.shouldBeSome(F.t(3, 2, 0, 1));
      subj4.push(5);
      r.value.shouldBeSome(F.t(3, 2, 0, 5));

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj1, subj2, subj3, subj4);
    }

    [Test]
    public void Zip5() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var subj4 = new Subject<int>();
      var subj5 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, subj4, subj5);
      var t = obs.pipeToRef();
      var r = t._1;
      subj1.push(1);
      r.value.shouldBeNone();
      subj2.push(1);
      r.value.shouldBeNone();
      subj3.push(1);
      r.value.shouldBeNone();
      subj4.push(1);
      r.value.shouldBeNone();
      subj5.push(1);
      r.value.shouldBeSome(F.t(1, 1, 1, 1, 1));
      subj2.push(2);
      r.value.shouldBeSome(F.t(1, 2, 1, 1, 1));
      subj3.push(0);
      r.value.shouldBeSome(F.t(1, 2, 0, 1, 1));
      subj1.push(3);
      r.value.shouldBeSome(F.t(3, 2, 0, 1, 1));
      subj4.push(5);
      r.value.shouldBeSome(F.t(3, 2, 0, 5, 1));
      subj5.push(6);
      r.value.shouldBeSome(F.t(3, 2, 0, 5, 6));
      
      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj1, subj2, subj3, subj4, subj5);
    }

    [Test]
    public void Zip6() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var subj4 = new Subject<int>();
      var subj5 = new Subject<int>();
      var subj6 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, subj4, subj5, subj6);
      var t = obs.pipeToRef();
      var r = t._1;
      subj1.push(1);
      r.value.shouldBeNone();
      subj2.push(1);
      r.value.shouldBeNone();
      subj3.push(1);
      r.value.shouldBeNone();
      subj4.push(1);
      r.value.shouldBeNone();
      subj5.push(1);
      r.value.shouldBeNone();
      subj6.push(1);
      r.value.shouldBeSome(F.t(1, 1, 1, 1, 1, 1));
      subj2.push(2);
      r.value.shouldBeSome(F.t(1, 2, 1, 1, 1, 1));
      subj3.push(0);
      r.value.shouldBeSome(F.t(1, 2, 0, 1, 1, 1));
      subj1.push(3);
      r.value.shouldBeSome(F.t(3, 2, 0, 1, 1, 1));
      subj4.push(5);
      r.value.shouldBeSome(F.t(3, 2, 0, 5, 1, 1));
      subj5.push(6);
      r.value.shouldBeSome(F.t(3, 2, 0, 5, 6, 1));
      subj6.push(7);
      r.value.shouldBeSome(F.t(3, 2, 0, 5, 6, 7));
      
      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj1, subj2, subj3, subj4, subj5, subj6);
    }
  }

  public class IObservableTestChangesOpt {
    [Test]
    public void WithDefaultEq() {
      var subj = new Subject<int>();
      var obs = subj.changesOpt();
      var t = obs.pipeToList();
      var list = t._1;
      subj.pushMany(1, 1, 2, 2, 3, 3);
      list.shouldEqual(F.list(
        F.t(Option<int>.None, 1),
        F.t(1.some(), 2),
        F.t(2.some(), 3)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj);
    }

    [Test]
    public void WithCustomEq() {
      var subj = new Subject<int>();
      var obs = subj.changesOpt((a, b) => false);
      var t = obs.pipeToList();
      var list = t._1;
      subj.pushMany(1, 1, 3, 3, 2, 2, 4, 4, 5, 5);
      list.shouldEqual(F.list(
        F.t(Option<int>.None, 1),
        F.t(1.some(), 1),
        F.t(1.some(), 3),
        F.t(3.some(), 3),
        F.t(3.some(), 2),
        F.t(2.some(), 2),
        F.t(2.some(), 4),
        F.t(4.some(), 4),
        F.t(4.some(), 5),
        F.t(5.some(), 5)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj);
    }
  }

  public class IObservableTestChanges {
    [Test]
    public void WithDefaultEq() {
      var subj = new Subject<int>();
      var obs = subj.changes();
      var t = obs.pipeToList();
      subj.pushMany(1, 1, 2, 2, 3, 3, 4, 4);
      t._1.shouldEqual(F.list(
        F.t(1, 2), F.t(2, 3), F.t(3, 4)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj);
    }

    [Test]
    public void WithCustomEq() {
      var subj = new Subject<int>();
      var obs = subj.changes((a, b) => false);
      var t = obs.pipeToList();
      subj.pushMany(1, 1, 2, 2, 3, 3, 4, 4);
      t._1.shouldEqual(F.list(
        F.t(1, 1), F.t(1, 2), F.t(2, 2), F.t(2, 3), F.t(3, 3), 
        F.t(3, 4), F.t(4, 4)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj);
    }
  }

  public class IOBservableTestChangedValues {
    [Test]
    public void WithDefaultEq() {
      var subj = new Subject<int>();
      var obs = subj.changedValues();
      var t = obs.pipeToList();
      var list = t._1;
      subj.push(1);
      list.shouldEqual(F.list(1));
      subj.pushMany(1, 1, 2, 2, 3, 3, 4, 4);
      list.shouldEqual(F.list(1, 2, 3, 4));

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj);
    }

    [Test]
    public void WithCustomEq() {
      var subj = new Subject<int>();
      var elems = new[] {1, 1, 2, 2, 3, 3, 4, 4};
      var obs = subj.changedValues((a, b) => false);
      var t = obs.pipeToList();
      subj.pushMany(elems);
      t._1.shouldEqual(elems.ToList());

      IObservableTestExts.testUnsubAndFinish(t._2, obs, subj);
    }
  }

  public class IObservableTestSkip {
    [Test]
    public void Finishing() {
      new Subject<int>().testFinishing(_ => _.skip(3));
      new Subject<int>().testFinishing(_ => _.skip(0));
    }

    [Test]
    public void Some() {
      var subj = new Subject<int>();
      var events = new List<int>();
      subj.skip(3).subscribe(events.Add);
      for (var idx = 1; idx <= 5; idx++) subj.push(idx);
      events.shouldEqual(F.list(4, 5));
    }

    [Test]
    public void None() {
      var subj = new Subject<int>();
      var events = new List<int>();
      subj.skip(0).subscribe(events.Add);
      for (var idx = 1; idx <= 5; idx++) subj.push(idx);
      events.shouldEqual(F.list(1, 2, 3, 4, 5));
    }
  }

  public class IObservableTestToRxVal {
    [Test]
    public void WithoutSubscribers() {
      var subj = new Subject<int>();
      var rx = subj.toRxVal(100);
      rx.value.shouldEqual(100);
      subj.push(10);
      rx.value.shouldEqual(10);
    }

    [Test]
    public void WithSubscribers() {
      var subj = new Subject<int>();
      var rx = subj.toRxVal(100);
      rx.value.shouldEqual(100);
      rx.pipeToRef().ua((r, sub) => {
        r.value.shouldBeSome(100);
        subj.push(10);
        r.value.shouldBeSome(10);
        rx.value.shouldEqual(10);

        sub.unsubscribe();
        subj.push(20);
        r.value.shouldBeSome(10);
        rx.value.shouldEqual(20);
      });
    }
  }

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