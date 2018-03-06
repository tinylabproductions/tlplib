using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
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
      lazy.get.forSideEffects();
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
      lazy.get.forSideEffects();
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
      lazy.get.forSideEffects();
      ftr.isCompleted.shouldBeTrue();
    }

    [Test]
    public void ItShouldHaveTheSameValueAsFuture() {
      var lazy = create();
      var ftr = lazy.asFuture();
      lazy.get.forSideEffects();
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
      invoked.shouldEqual(0u, $"it should'nt invoke {nameof(ftr.onComplete)} before .get.forSideEffects()");
      lazy.get.forSideEffects();
      invoked.shouldEqual(1u, $"it should invoke {nameof(ftr.onComplete)} after .get.forSideEffects()");
      lazy.get.forSideEffects();
      invoked.shouldEqual(1u, $"it should not invoke {nameof(ftr.onComplete)} twice");
    }
  }

  public class LazySpecification : ImplicitSpecification {
    class Base {}
    class Child : Base {}

    [Test]
    public void upcast() => describe(() => {
      var obj = new Child();
      var lazy = let(() => F.lazy(() => obj));
      var upcasted = @let(() => lazy.value.upcast<Child, Base>());

      when["#" + nameof(lazy.value.isCompleted)] = () => {
        it["should transmit non-completion"] = () => upcasted.value.isCompleted.shouldBeFalse();
        it["should transmit completion"] = () => {
          lazy.value.get.forSideEffects();
          upcasted.value.isCompleted.shouldBeTrue();
        };
      };

      when["#" + nameof(lazy.value.onComplete)] = () => {
        var result = @let(Option<Base>.None);
        beforeEach += () => upcasted.value.onComplete(b => result.value = b.some());

        it["should transmit non-completion"] = () => result.value.shouldBeNone();
        it["should transmit completion"] = () => {
          lazy.value.get.forSideEffects();
          result.value.shouldBeSome(obj);
        };
      };

      when["#" + nameof(lazy.value.value)] = () => {
        it["should transmit non-completion"] = () => upcasted.value.value.shouldBeNone();
        it["should transmit completion"] = () => {
          lazy.value.get.forSideEffects();
          upcasted.value.value.shouldBeSome(obj);
        };
      };
    });
  }
}
