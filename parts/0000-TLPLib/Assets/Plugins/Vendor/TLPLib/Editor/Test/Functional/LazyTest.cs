using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class NotReallyLazyTestAsFuture {
    const string value = "foo";
    static NotReallyLazyVal<string> create() => new NotReallyLazyVal<string>(value);

    [Test]
    public void ItShouldCompleteBeforeWeGetTheValue() {
      var lazy = create();
      var ftr = lazy.asFuture();
      ftr.isCompleted.shouldBeTrue();
      lazy.getM();
      ftr.isCompleted.shouldBeTrue();
    }

    [Test]
    public void ItShouldHaveTheSameValueAsFuture() => 
      create().asFuture().value.shouldBeSome(value);

    [Test]
    public void ItShouldEmitOnCompleteInstantly() {
      var lazy = create();
      var ftr = lazy.asFuture();
      var invoked = 0u;
      ftr.onComplete(v => {
        v.shouldEqual(value);
        invoked++;
      });
      invoked.shouldEqual(1u, $"it should immediately invoke {nameof(ftr.onComplete)}");
      lazy.getM();
      invoked.shouldEqual(1u, $"it should not invoke {nameof(ftr.onComplete)} twice");
    }
  }

  public class LazyImplTestAsFuture {
    const string value = "foo";
    static LazyValImpl<string> create() => new LazyValImpl<string>(() => value);

    [Test]
    public void ItShouldNotCompleteUntilWeGetTheValue() {
      var lazy = create();
      var ftr = lazy.asFuture();
      ftr.isCompleted.shouldBeFalse();
      lazy.getM();
      ftr.isCompleted.shouldBeTrue();
    }

    [Test]
    public void ItShouldHaveTheSameValueAsFuture() {
      var lazy = create();
      var ftr = lazy.asFuture();
      lazy.getM();
      ftr.value.shouldBeSome(value);
    }

    [Test]
    public void ItShouldEmmitOnCompleteAfterGet() {
      var lazy = create();
      var ftr = lazy.asFuture();
      var invoked = 0u;
      ftr.onComplete(v => {
        v.shouldEqual(value);
        invoked++;
      });
      invoked.shouldEqual(0u, $"it should'nt invoke {nameof(ftr.onComplete)} before .getM()");
      lazy.getM();
      invoked.shouldEqual(1u, $"it should invoke {nameof(ftr.onComplete)} after .getM()");
      lazy.getM();
      invoked.shouldEqual(1u, $"it should not invoke {nameof(ftr.onComplete)} twice");
    }
  }
}
