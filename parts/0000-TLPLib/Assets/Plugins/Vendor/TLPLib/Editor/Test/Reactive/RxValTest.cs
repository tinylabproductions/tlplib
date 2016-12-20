using System;
using System.Linq;
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

  public class RxValTestAnyOf {
    [Test]
    public void WhenEmptySearchForTrue() =>
      Enumerable.Empty<IRxVal<bool>>().anyOf(searchFor: true).value.shouldBeFalse();

    [Test]
    public void WhenEmptySearchForFalse() =>
      Enumerable.Empty<IRxVal<bool>>().anyOf(searchFor: false).value.shouldBeFalse();

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