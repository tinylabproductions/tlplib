using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
  public class RxValTest : ImplicitSpecification {
    [Test]
    public void ctor() => describe(() => {
      var mapperInvocations = 0;
      var actionInvocations = 0;
      var lastActionResult = 0;
      IRxRef<Tpl<int, int>> src = null;
      IRxVal<int> rx = null;

      beforeEach += () => {
        mapperInvocations = 0;
        actionInvocations = 0;
        src = RxRef.a(F.t(10, 0));
        rx = new RxVal<int>(
          -11,
          setValue => src.subscribeWithoutEmit(tracker, t => {
            mapperInvocations++;
            setValue(t._1 + t._2 + 1);
          })
        );
        rx.subscribe(tracker, i => {
          actionInvocations++;
          lastActionResult = i;
        });
      };

      on["creation"] = () => {
        it["should create a subscription to source"] = () => src.subscribers.shouldEqual(1);
        it["should not invoke mapper"] = () => mapperInvocations.shouldEqual(0);
        it["should have specified value"] = () => rx.value.shouldEqual(-11);
        it["should invoke action"] = () => actionInvocations.shouldEqual(1);
        it["should invoke action with current value"] = () => lastActionResult.shouldEqual(-11);

        when["source changes"] = () => {
          beforeEach += () => src.value = F.t(2, 3);

          it["should invoke mapper"] = () => mapperInvocations.shouldEqual(1);
          it["should update value"] = () => rx.value.shouldEqual(6);
          it["should invoke action"] = () => actionInvocations.shouldEqual(2);
          it["should invoke action with recomputed value"] = () => lastActionResult.shouldEqual(6);

          when["source changes, but transformation result is the same"] = () => {
            beforeEach += () => src.value = F.t(3, 2);

            it["should invoke mapper"] = () => mapperInvocations.shouldEqual(2);
            it["should keep the value same"] = () => rx.value.shouldEqual(6);
            it["should not invoke action"] = () => actionInvocations.shouldEqual(2);
          };
        };
      };
    });

    [Test] public void map() => describe(_ => {
      var mapperInvocations = 0;
      IRxRef<int> src = null;
      IRxVal<int> rx = null;

      _.beforeEach += () => {
        mapperInvocations = 0;
        src = RxRef.a(10);
        rx = src.map(i => {
          mapperInvocations++;
          return i + 1;
        });
      };

      _.when["not subscribed"] = () => {
        _.it["should invoke mapper once on creation"] = () => {
          mapperInvocations.shouldEqual(1);
        };

        _.it["should invoke mapper on source changes"] = () => {
          src.value++;
          mapperInvocations.shouldEqual(2);
        };

        _.it["should not invoke mapper upon requesting the value"] = () => {
          rx.value.forSideEffects();
          mapperInvocations.shouldEqual(1);
        };

        _.it["should map the value correctly"] = () => {
          rx.value.shouldEqual(11);
        };

        _.it["should map the value after change correctly"] = () => {
          src.value = 20;
          rx.value.shouldEqual(21);
        };
      };

      void subscribedSpec(int initialValue) {
        List<int> list = null;
        _.beforeEach += () => list = rx.pipeToList(tracker)._1;

        void createSpec(int current, int[] values) {
          _.it["should have the mapped value"] = () => rx.value.shouldEqual(current);
          _.it["should push the mapped value"] = () => list.shouldEqualEnum(values);
          _.it["should not invoke mapper again upon requesting value"] =
            () => code(() => rx.value).shouldNotChange(() => mapperInvocations);
        }

        createSpec(initialValue + 1, new[] { initialValue + 1 });

        _.when["source has been updated"] = () => {
          _.beforeEach += () => src.value++;
          createSpec(initialValue + 2, new[] { initialValue + 1, initialValue + 2 });

          _.when["source has been updated again"] = () => {
            _.beforeEach += () => src.value++;
            createSpec(initialValue + 3, new[] { initialValue + 1, initialValue + 2, initialValue + 3 });
          };
        };
      }

      _.when["subscribed"] = () => subscribedSpec(10);

      _.when["subscribed, changed and unsubscribed"] = () => {
        _.beforeEach += () => {
          var (list, subscription) = rx.pipeToList(tracker);
          src.value++;
          subscription.unsubscribe();
          src.value++;

          list.shouldEqualEnum(11, 12);
          rx.value.shouldEqual(13);
        };

        subscribedSpec(12);
      };
    });

    [Test]
    public void flatMap() => describe(() => {
      var mapperInvocations = 0;
      IRxRef<int> src = null;
      IRxRef<string> interim1 = null, interim2 = null;
      IRxVal<string> rx = null;

      beforeEach += () => {
        mapperInvocations = 0;
        src = RxRef.a(10);
        interim1 = RxRef.a("i1");
        interim2 = RxRef.a("i2");
        rx = src.flatMap(i => {
          mapperInvocations++;
          return i % 2 == 0 ? interim1 : interim2;
        });
      };

      it["should invoke mapper once initially"] = () => mapperInvocations.shouldEqual(1);
      it["should have the proper initial value"] = () => rx.value.shouldEqual("i1");
      it["should not invoke mapper if interim value changes"] = () => {
        rx.value.shouldEqual("i1");
        code(() => {
          interim1.value = "i1_1";
          rx.value.shouldEqual("i1_1");
        }).shouldNotChange(() => mapperInvocations);
      };

      when["src changes"] = () => {
        beforeEach += () => src.value++;
        it["should invoke mapper"] = () => mapperInvocations.shouldEqual(2);
        it["should have correct value"] = () => rx.value.shouldEqual("i2");

        when["second interim changes"] = () => {
          beforeEach += () => interim2.value = "i2_2";
          it["should not invoke mapper"] = () => mapperInvocations.shouldEqual(2);
          it["should have correct value"] = () => rx.value.shouldEqual("i2_2");
        };

        when["src changes back and first interim is changed"] = () => {
          beforeEach += () => {
            interim1.value = "i1_3";
            src.value++;
          };

          it["should invoke mapper"] = () => mapperInvocations.shouldEqual(3);
          it["should have correct value"] = () => rx.value.shouldEqual("i1_3");
        };
      };
    });

    [Test] public void subscribeForOneEvent() => describe(() => {
      var actionInvocations = 0;
      var rx = RxRef.a(false);
      var sub = rx.subscribeForOneEvent(new DisposableTracker(), _ => actionInvocations++);

      it["should invoke action"] = () => actionInvocations.shouldEqual(1);
      it["should be unsubscribed"] = () => sub.isSubscribed.shouldBeFalse();

      when["changing value"] = () => {
        beforeEach += () => {
          rx.value = true;
        };

        it["should not invoke action"] = () => actionInvocations.shouldEqual(1);
      };
    });
  }

  public class RxValTestFirstThat {
    [Test]
    public void WhenEmpty() =>
      Enumerable.Empty<IRxVal<int>>().anyThat(_ => true).value.shouldBeNone();

    [Test]
    public void WhenFromSingleItem() {
      var rx = RxRef.a(3);
      var rx2 = new[] {rx}.anyThat(i => i % 2 != 0);
      rx2.value.shouldBeSome(3);
      rx.value = 2;
      rx2.value.shouldBeNone();
      rx.value = 5;
      rx2.value.shouldBeSome(5);
    }

    [Test]
    public void WhenMultipleItems() {
      bool predicate(int i) => i % 2 != 0;
      bool matchPredicate(Option<int> _) => _.exists(predicate);
      var rx1 = RxRef.a(3);
      var rx2 = RxRef.a(4);
      var dst = new[] {rx1, rx2}.anyThat((Fn<int, bool>) predicate);
      dst.value.shouldMatch(matchPredicate);
      rx1.value = 2;
      dst.value.shouldBeNone();
      rx2.value = 5;
      dst.value.shouldMatch(matchPredicate);
      rx1.value = 1;
      dst.value.shouldMatch(matchPredicate);
    }
  }

  public class RxValTestAnyOf : Specification {
    [Test]
    public void anyOf() => describe(_ => {
      _.when["empty"] = () => {
        var e = Enumerable.Empty<IRxVal<bool>>();

        _.it["should return false when searching for true"] = () =>
          e.anyOf(searchFor: true).value.shouldBeFalse();

        _.it["should return false when searching for false"] = () =>
          e.anyOf(searchFor: false).value.shouldBeFalse();
      };
    });

    [Test]
    public void WhenFromSingleItemSearchForTrue() {
      var rx = RxRef.a(true);
      var dst = new[] {rx}.anyOf(searchFor: true);
      dst.value.shouldBeTrue();
      rx.value = false;
      dst.value.shouldBeFalse();
      rx.value = true;
      dst.value.shouldBeTrue();
    }

    [Test]
    public void WhenFromSingleItemSearchForFalse() {
      var rx = RxRef.a(false);
      var dst = new[] {rx}.anyOf(searchFor: false);
      dst.value.shouldBeTrue();
      rx.value = true;
      dst.value.shouldBeFalse();
      rx.value = false;
      dst.value.shouldBeTrue();
    }

    [Test]
    public void WhenMultipleItemsSearchForTrue() {
      var rx1 = RxRef.a(true);
      var rx2 = RxRef.a(false);
      var dst = new[] {rx1, rx2}.anyOf(searchFor: true);
      dst.value.shouldBeTrue();
      rx1.value = false;
      dst.value.shouldBeFalse();
      rx2.value = true;
      dst.value.shouldBeTrue();
      rx1.value = true;
      dst.value.shouldBeTrue();
    }

    [Test]
    public void WhenMultipleItemsSearchForFalse() {
      var rx1 = RxRef.a(false);
      var rx2 = RxRef.a(true);
      var dst = new[] {rx1, rx2}.anyOf(searchFor: false);
      dst.value.shouldBeTrue();
      rx1.value = true;
      dst.value.shouldBeFalse();
      rx2.value = false;
      dst.value.shouldBeTrue();
      rx1.value = false;
      dst.value.shouldBeTrue();
    }
  }

  public class RxValTestAnyDefined {
    [Test]
    public void WhenEmpty() =>
      Enumerable.Empty<IRxVal<Option<int>>>().anyDefined().value.shouldBeNone();

    [Test]
    public void WhenFromSingleItem() {
      var rx = RxRef.a(3.some());
      var dst = new[] {rx}.anyDefined();
      dst.value.shouldBeSome(3);
      rx.value = Option<int>.None;
      dst.value.shouldBeNone();
      rx.value = 4.some();
      dst.value.shouldBeSome(4);
    }

    [Test]
    public void WhenMultipleItems() {
      var rx1 = RxRef.a(3.some());
      var rx2 = RxRef.a(Option<int>.None);
      var dst = new[] {rx1, rx2}.anyDefined();
      dst.value.shouldBeSome(3);
      rx1.value = Option<int>.None;
      dst.value.shouldBeNone();
      rx2.value = 4.some();
      dst.value.shouldBeSome(4);
      rx1.value = 1.some();
      dst.value.shouldBeAnySome();
      rx2.value = Option<int>.None;
      dst.value.shouldBeSome(1);
    }
  }
}