using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Reactive {
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
      Fn<int, bool> predicate = i => i % 2 != 0;
      Fn<Option<int>, bool> matchPredicate = _ => _.exists(predicate);
      var rx1 = RxRef.a(3);
      var rx2 = RxRef.a(4);
      var dst = new[] {rx1, rx2}.anyThat(predicate);
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
      _.when("empty", () => {
        var e = Enumerable.Empty<IRxVal<bool>>();

        _.it(
          "should return false when searching for true", 
          () => e.anyOf(searchFor: true).value.shouldBeFalse()
        );

        _.it(
          "should return false when searching for false",
          () => e.anyOf(searchFor: false).value.shouldBeFalse()
        );
      });
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
  
  public class RxValTest : Specification {
    [Test] public void map() => describe(_ => {
      var mapperInvocations = _.beforeEach(() => 0);
      var src = _.beforeEach(() => RxRef.a(10));
      var rx = _.beforeEach(() => src.value.map(i => {
        mapperInvocations.value++;
        return i + 1;
      }));

      _.when("not subscribed", () => {
        _.it("should not invoke mapper on creation", () => {
          mapperInvocations.value.shouldEqual(0);
        });

        _.it("should not invoke mapper on source changes", () => {
          src.value.value++;
          mapperInvocations.value.shouldEqual(0);
        });

        _.it("should invoke mapper upon requesting the value", () => {
          rx.value.value.forSideEffects();
          mapperInvocations.value.shouldEqual(1);
        });

        _.it(
          "should invoke mapper only once upon value request even if source changed multiple times",
          () => {
            src.value.value++;
            src.value.value++;
            rx.value.value.forSideEffects();
            mapperInvocations.value.shouldEqual(1);
          }
        );

        _.it(
          "should invoke mapper again if a change happened after last value request",
          () => {
            rx.value.value.forSideEffects();
            src.value.value++;
            rx.value.value.forSideEffects();
            mapperInvocations.value.shouldEqual(2);
          }
        );

        _.it("should map the correctly", () => {
          rx.value.value.shouldEqual(11);
        });

        _.it("should map the value after change correctly", () => {
          src.value.value = 20;
          rx.value.value.shouldEqual(21);
        });
      });

      Fn<Val<List<int>>, Act<int, int[]>> createSpec = list => (current, values) => {
        _.it("should have the mapped value", () => rx.value.value.shouldEqual(current));
        _.it("should push the mapped value", () => list.value.shouldEqualEnum(values));
        _.it(
          "should not invoke mapper again upon requesting value",
          () => code(() => 
            rx.value.value
          ).shouldNotChange(mapperInvocations)
        );
      };

      Act<int> subscribedSpec = initialValue => {
        var list = _.beforeEach(() => rx.value.pipeToList()._1);
        var spec = createSpec(list);

        spec(initialValue + 1, new[] { initialValue + 1 });

        _.when("source has been updated", () => {
          _.beforeEach(() => src.value.value++);
          spec(initialValue + 2, new[] { initialValue + 1, initialValue + 2 });

          _.when("source has been updated again", () => {
            _.beforeEach(() => src.value.value++);
            spec(initialValue + 3, new[] { initialValue + 1, initialValue + 2, initialValue + 3 });
          });
        });
      };

      _.when("subscribed", () => subscribedSpec(10));

      _.when("subscribed, changed and unsubscribed", () => {
        _.beforeEach(() => {
          var x = rx.value.pipeToList();
          src.value.value++;
          x._2.unsubscribe();
          src.value.value++;

          x._1.shouldEqualEnum(11, 12);
          rx.value.value.shouldEqual(13);
        });

        subscribedSpec(12);
      });
    });

    [Test]
    public void flatMap() => describe(_ => {
      var mapperInvocations = _.beforeEach(() => 0);
      var src = _.beforeEach(() => RxRef.a(10));
      var interim1 = _.beforeEach(() => RxRef.a("i1"));
      var interim2 = _.beforeEach(() => RxRef.a("i2"));
      var rx = _.beforeEach(() => src.value.flatMap(i => {
        mapperInvocations.value++;
        return i % 2 == 0 ? interim1.value : interim2.value;
      }));

      _.when("not subscribed", () => {
        _.it("should not invoke mapper initially", () => mapperInvocations.value.shouldEqual(0));
        _.it("should have the proper initial value", () => rx.value.value.shouldEqual("i1"));
        _.it(
          "should invoke mapper once when asking for a value", 
          () => code(() => rx.value.value).shouldChange(mapperInvocations).by(1)
        );
        _.it(
          "should not invoke mapper again if value did not change",
          () => {
            rx.value.value.forSideEffects();
            code(() => rx.value.value).shouldNotChange(mapperInvocations);
          }
        );
        _.it(
          "should not invoke mapper if interim value changes",
          () => code(() => {
            interim1.value.value = "i1_1";
            rx.value.value.shouldEqual("i1_1");
          }).shouldNotChange(mapperInvocations)
        );
      });

      _.when("subscribed", () => {
        
      });
    });
  }
}