using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Collection;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class IObservableTest : ImplicitSpecification {
    [Test]
    public void subscriptionCounting() => describe(() => {
      when["simple"] = () => {
        it["should work"] = () => {
          var o = new Subject<Unit>();
          var s1 = o.subscribe(tracker, _ => { });
          var s2 = o.subscribe(tracker, _ => { });
          o.subscribers.shouldEqual(2);
          s1.unsubscribe();
          o.subscribers.shouldEqual(1);
          s1.unsubscribe();
          o.subscribers.shouldEqual(1);
          s2.unsubscribe();
          o.subscribers.shouldEqual(0);
          var s3 = o.subscribe(tracker, _ => { });
          o.subscribers.shouldEqual(1);
          s3.unsubscribe();
          o.subscribers.shouldEqual(0);
        };
      };

      when["nested"] = () => {
        it["should work"] = () => {
          var o = new Subject<Unit>();
          var o2 = o.map(_ => 1);
          Assert.AreEqual(0, o.subscribers);
          Assert.AreEqual(0, o2.subscribers);
          var s = o2.subscribe(tracker, _ => { });
          Assert.AreEqual(1, o.subscribers);
          Assert.AreEqual(1, o2.subscribers);
          s.unsubscribe();
          Assert.AreEqual(0, o.subscribers);
          Assert.AreEqual(0, o2.subscribers);
        };
      };
    });

    [Test]
    public void subscribe() => describe(() => {
      it["should add created subscription to the given tracker"] = () => {
        var s = new Subject<Unit>();
        var t = new DisposableTracker();
        var sub = s.subscribe(t, _ => { });
        t.trackedDisposables.shouldContain(_ => ReferenceEquals(_.disposable, sub));
      };

      it["should not invoke the subscription function upon subscribing"] = () => {
        var s = new Subject<Unit>();
        var counter = 0;
        s.subscribe(tracker, _ => counter++);
        counter.shouldEqual(0);
      };

      it["should invoke the subscription function upon event"] = () => {
        var s = new Subject<Unit>();
        var counter = 0;
        s.subscribe(tracker, _ => counter++);
        s.push(F.unit);
        counter.shouldEqual(1);
      };

      when["subscribing from inside of other event"] = () => {
        it["should work"] = () => {
          var subject = new Subject<Unit>();
          var pushedOuter = 0;
          var pushedInner = 0;
          subject.subscribe(tracker, _ => {
            pushedOuter++;
            subject.subscribe(tracker, __ => pushedInner++);
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
        };
      };

      when["pushing from inside of other event"] = () => {
        it["should work"] = () => {
          var subj = new Subject<int>();
          var list = F.emptyList<Tpl<int, char>>();
          subj.subscribe(tracker, a => {
            list.Add(F.t(a, 'a'));
            if (a == 0) {
              subj.push(1);
              subj.push(2);
            }
          });
          subj.subscribe(tracker, a => { list.Add(F.t(a, 'b')); });
          list.shouldBeEmpty();
          subj.push(0);
          list.shouldEqual(F.list(
            F.t(0, 'a'), F.t(0, 'b'),
            F.t(1, 'a'), F.t(1, 'b'),
            F.t(2, 'a'), F.t(2, 'b')
          ));
        };
      };

      when["unsubscribing after receiving an event"] = () => {
        it["should successfully unsubscribe"] = () => {
          var subject = new Subject<Unit>();
          var called = 0;
          subject.subscribe(tracker, (_, subscription) => {
            called += 1;
            subscription.unsubscribe();
          });
          subject.push(F.unit);
          subject.push(F.unit);
          Assert.AreEqual(1, called);
        };
      };
    });

    [Test]
    public void toFuture() {
      var subj = new Subject<int>();
      var f = subj.toFuture(tracker);
      f.value.shouldBeNone();
      subj.push(1);
      f.value.shouldBeSome(1);
      subj.subscribers.shouldEqual(0, "it should unsubscribe after completing the future");
    }

    [Test]
    public void extract() => describe(() => {
      Subject<int> source = null;
      beforeEach += () => source = new Subject<int>();

      Future<IRxObservable<int>> future;
      IRxObservable<int> extracted = null;

      Tpl<Ref<List<int>>, Ref<ISubscription>> pipe() {
        var list = Ref.a(new List<int>());
        var sub = Ref.a(Subscription.empty);
        beforeEach += () => {
          var (_list, _sub) = extracted.pipeToList(tracker);
          list.value = _list;
          sub.value = _sub;
        };
        return F.t(list, sub);
      }

      void testProxying(Ref<List<int>> list) {
        it["should subscribe to source"] = () => source.subscribers.shouldEqual(1);
        it["should proxy events"] = () => {
          list.value.Clear();
          foreach (var a in new [] {1, 2, 3}) source.push(a);
          list.value.shouldEqualEnum(1, 2, 3);
        };
      }

      void testNonProxying(Ref<List<int>> list) {
        it["should unsubscribe from source"] = () => source.subscribers.shouldEqual(0);
        it["should not proxy events"] = () => {
          list.value.Clear();
          foreach (var a in new[] {1, 2, 3}) source.push(a);
          list.value.shouldBeEmpty();
        };
      }

      void testUnsubResub(Ref<List<int>> list, Ref<ISubscription> sub) {
        then["we unsubscribe from observable"] = () => {
          beforeEach += () => sub.value.unsubscribe();
          testNonProxying(list);

          then["we resubscribe from observable"] = () => {
            var (_list, _) = pipe();
            testProxying(_list);
          };
        };
      }

      when["future is completed before we get a subscriber"] = () => {
        beforeEach += () => {
          future = Future.successful(source.asObservable());
          extracted = future.extract();
        };

        it["should not subscribe to source"] = () => source.subscribers.shouldEqual(0);

        then["we subscribe to observable"] = () => {
          var (list, sub) = pipe();
          testProxying(list);
          testUnsubResub(list, sub);
        };
      };

      when["future is completed after we have a subscriber"] = () => {
        Promise<IRxObservable<int>> promise = null;
        beforeEach += () => {
          future = Future.async(out promise);
          extracted = future.extract();
        };
        var (list, sub) = pipe();
        beforeEach += () => promise.complete(source);

        testProxying(list);
        testUnsubResub(list, sub);
      };
    });
  }

  public class IObservableTestMap : TestBase {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.map(i => i * 2);
      var (list, sub) = obs.pipeToList(tracker);
      foreach (var a in new[] {1, 2, 3, 4, 5}) subj.push(a);
      list.shouldEqual(F.list(2, 4, 6, 8, 10));
      sub.testUnsubscription(subj);
    }
  }

  public class IObservableTestDiscardValue : TestBase {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.discardValue();
      var (evts, sub) = obs.countEvents(tracker);
      evts.value.shouldEqual(0u);
      foreach (var a in new[] {1, 2, 3, 4}) subj.push(a);
      evts.value.shouldEqual(4u);
      sub.testUnsubscription(subj);
    }
  }

  public class IObservableTestFlatMap : TestBase {
    [Test]
    public void TestIEnumerable() {
      var subj = new Subject<int>();
      var obs = subj.flatMap(i => Enumerable.Range(0, i));
      var (list, sub) = obs.pipeToList(tracker);
      foreach (var a in new[] {1, 2, 3}) subj.push(a);
      list.shouldEqual(F.list(
        0,
        0, 1,
        0, 1, 2
      ));
      sub.testUnsubscription(subj);
    }

    [Test]
    public void TestIObservable() {
      var subj = new Subject<int>();
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj.flatMap(i => i % 2 == 0 ? subj1 : subj2);
      var t = obs.pipeToList(tracker);
      var list = t._1;
      var sub = t._2;

      void checkSubs(int s1, int s2) {
        subj1.subscribers.shouldEqual(s1);
        subj2.subscribers.shouldEqual(s2);
      }

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
      checkSubs(0, 0);
    }

    [Test]
    public void TestFuture() {
      var gateF = Future<Unit>.async(out var gateP);
      var subj = new Subject<int>();
      var obs = subj.flatMap(i => gateF.map(_ => i));
      var t = obs.pipeToList(tracker);
      var list = t._1;
      var sub = t._2;
      foreach (var a in new[] {1, 2, 3}) subj.push(a);
      list.shouldBeEmpty();
      gateP.complete(F.unit);
      list.shouldEqual(F.list(1, 2, 3));
      sub.testUnsubscription(subj);
    }
  }

  public class IObservableTestFilter : TestBase {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.filter(i => i % 2 == 0);
      var t = obs.pipeToList(tracker);
      var list = t._1;
      var sub = t._2;
      foreach (var i in Enumerable.Range(0, 5)) subj.push(i);
      list.shouldEqual(F.list(0, 2, 4));
      sub.testUnsubscription(subj);
    }
  }

  public class IObservableTestCollect : TestBase {
    [Test]
    public void Test() {
      var subj = new Subject<int>();
      var obs = subj.collect(i => (i % 2 == 0).opt(i * 2));
      var t = obs.pipeToList(tracker);
      var list = t._1;
      var sub = t._2;
      foreach (var a in new[] {0, 1, 2, 3, 4, 5, 6}) subj.push(a);
      list.shouldEqual(F.list(0, 4, 8, 12));
      sub.testUnsubscription(subj);
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

  public class IObservableTestBuffer : TestBase {
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

    void test<C>(
      Func<Subject<int>, IRxObservable<C>> createObs, Func<List<int>, C> createCollection
    ) where C : IEnumerable<int> {
      var subj = new Subject<int>();
      var obs = createObs(subj);
      var t = obs.pipeToRef(tracker);
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
    }
  }

  // TODO: test timeBuffer with integration tests

  public class IObservableTestJoin : TestBase {
    [Test]
    public void Test() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj1.join(subj2);
      var (list, sub) = obs.pipeToList(tracker);
      subj1.push(1);
      subj2.push(2);
      subj1.push(3);
      subj2.push(4);
      list.shouldEqual(F.list(1, 2, 3, 4));
      sub.testUnsubscription(subj1, subj2);
    }
  }

  public class IObservableTestJoinAll : TestBase {
    class A {}
    class B : A {}

    [Test]
    public void TestOneToMany() {
      var aSubj = new Subject<A>();
      var otherSubjects = ImmutableList.Create(new Subject<B>(), new Subject<B>());
      var otherObservables = otherSubjects.Select(s => s.map(_ => (A)_)).ToImmutableList();
      var allObservables = aSubj.Yield<IRxObservable>().Concat(otherObservables).ToArray();

      var obs = aSubj.joinAll(otherObservables);
      var t = obs.pipeToList(tracker);
      var a = new A();
      var b = new B();

      aSubj.push(a);
      foreach (var subj in otherSubjects) subj.push(b);
      t._1.shouldEqual(F.list(a, b, b));
      t._2.testUnsubscriptionC(allObservables);
    }

    [Test]
    public void TestCollection() {
      var subjs = new[] {new Subject<int>(), new Subject<int>(), new Subject<int>()};
      var obs = subjs.joinAll();
      var t = obs.pipeToList(tracker);
      foreach (var subj in subjs) subj.push(1);
      t._1.shouldEqual(F.list(1, 1, 1));
      t._2.testUnsubscriptionC(subjs);
    }
  }

  public class IObservableTestJoinDiscard : TestBase {
    [Test]
    public void Test() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj1.joinDiscard(subj2);
      var t = obs.countEvents(tracker);
      var evts = t._1;
      foreach (var a in new[] {1, 2, 3}) subj1.push(a);
      foreach (var a in new[] {1, 2, 3}) subj2.push(a);
      evts.value.shouldEqual(6u);

      IObservableTestExts.testUnsubAndFinish(t._2, subj1, subj2);
    }
  }

  public class IObservableTestZip : TestBase {
    [Test]
    public void Zip2() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var obs = subj1.zip(subj2, F.t);
      var t = obs.pipeToRef(tracker);
      var r = t._1;
      subj1.push(1);
      r.value.shouldBeNone();
      subj2.push(1);
      r.value.shouldBeSome(F.t(1, 1));
      subj2.push(2);
      r.value.shouldBeSome(F.t(1, 2));
      subj1.push(0);
      r.value.shouldBeSome(F.t(0, 2));

      IObservableTestExts.testUnsubAndFinish(t._2, subj1, subj2);
    }

    [Test]
    public void Zip3() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, F.t);
      var t = obs.pipeToRef(tracker);
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

      IObservableTestExts.testUnsubAndFinish(t._2, subj1, subj2, subj3);
    }

    [Test]
    public void Zip4() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var subj4 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, subj4, F.t);
      var t = obs.pipeToRef(tracker);
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

      IObservableTestExts.testUnsubAndFinish(t._2, subj1, subj2, subj3, subj4);
    }

    [Test]
    public void Zip5() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var subj4 = new Subject<int>();
      var subj5 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, subj4, subj5, F.t);
      var t = obs.pipeToRef(tracker);
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

      IObservableTestExts.testUnsubAndFinish(t._2, subj1, subj2, subj3, subj4, subj5);
    }

    [Test]
    public void Zip6() {
      var subj1 = new Subject<int>();
      var subj2 = new Subject<int>();
      var subj3 = new Subject<int>();
      var subj4 = new Subject<int>();
      var subj5 = new Subject<int>();
      var subj6 = new Subject<int>();
      var obs = subj1.zip(subj2, subj3, subj4, subj5, subj6, F.t);
      var t = obs.pipeToRef(tracker);
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

      IObservableTestExts.testUnsubAndFinish(t._2, subj1, subj2, subj3, subj4, subj5, subj6);
    }
  }

  public class IObservableTestChangesOpt : TestBase {
    [Test]
    public void WithDefaultEq() {
      var subj = new Subject<int>();
      var obs = subj.changesOpt();
      var t = obs.pipeToList(tracker);
      var list = t._1;
      foreach (var a in new[] {1, 1, 2, 2, 3, 3}) subj.push(a);
      list.shouldEqual(F.list(
        F.t(Option<int>.None, 1),
        F.t(1.some(), 2),
        F.t(2.some(), 3)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, subj);
    }

    [Test]
    public void WithCustomEq() {
      var subj = new Subject<int>();
      var obs = subj.changesOpt((a, b) => false);
      var t = obs.pipeToList(tracker);
      var list = t._1;
      foreach (var a in new[] {1, 1, 3, 3, 2, 2, 4, 4, 5, 5}) subj.push(a);
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

      IObservableTestExts.testUnsubAndFinish(t._2, subj);
    }
  }

  public class IObservableTestChanges : TestBase {
    [Test]
    public void WithDefaultEq() {
      var subj = new Subject<int>();
      var obs = subj.changes();
      var t = obs.pipeToList(tracker);
      foreach (var a in new[] {1, 1, 2, 2, 3, 3, 4, 4}) subj.push(a);
      t._1.shouldEqual(F.list(
        F.t(1, 2), F.t(2, 3), F.t(3, 4)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, subj);
    }

    [Test]
    public void WithCustomEq() {
      var subj = new Subject<int>();
      var obs = subj.changes((a, b) => false);
      var t = obs.pipeToList(tracker);
      foreach (var a in new[] {1, 1, 2, 2, 3, 3, 4, 4}) subj.push(a);
      t._1.shouldEqual(F.list(
        F.t(1, 1), F.t(1, 2), F.t(2, 2), F.t(2, 3), F.t(3, 3),
        F.t(3, 4), F.t(4, 4)
      ));

      IObservableTestExts.testUnsubAndFinish(t._2, subj);
    }
  }

  public class IOBservableTestChangedValues : TestBase {
    [Test]
    public void WithDefaultEq() {
      var subj = new Subject<int>();
      var obs = subj.changedValues();
      var t = obs.pipeToList(tracker);
      var list = t._1;
      subj.push(1);
      list.shouldEqual(F.list(1));
      foreach (var a in new[] {1, 1, 2, 2, 3, 3, 4, 4}) subj.push(a);
      list.shouldEqual(F.list(1, 2, 3, 4));

      IObservableTestExts.testUnsubAndFinish(t._2, subj);
    }

    [Test]
    public void WithCustomEq() {
      var subj = new Subject<int>();
      var elems = new[] {1, 1, 2, 2, 3, 3, 4, 4};
      var obs = subj.changedValues((a, b) => false);
      var t = obs.pipeToList(tracker);
      foreach (var a in elems) subj.push(a);
      t._1.shouldEqual(elems.ToList());

      IObservableTestExts.testUnsubAndFinish(t._2, subj);
    }
  }

  public class IObservableTestSkip : TestBase {
    [Test]
    public void Some() {
      var subj = new Subject<int>();
      var events = new List<int>();
      subj.skip(3).subscribe(tracker, events.Add);
      for (var idx = 1; idx <= 5; idx++) subj.push(idx);
      events.shouldEqual(F.list(4, 5));
    }

    [Test]
    public void None() {
      var subj = new Subject<int>();
      var events = new List<int>();
      subj.skip(0).subscribe(tracker, events.Add);
      for (var idx = 1; idx <= 5; idx++) subj.push(idx);
      events.shouldEqual(F.list(1, 2, 3, 4, 5));
    }
  }

  public class IObservableTestToRxVal : TestBase {
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
      var (r, sub) = rx.pipeToRef(tracker);
      r.value.shouldBeSome(100);
      subj.push(10);
      r.value.shouldBeSome(10);
      rx.value.shouldEqual(10);

      sub.unsubscribe();
      subj.push(20);
      r.value.shouldBeSome(10);
      rx.value.shouldEqual(20);
    }
  }
}