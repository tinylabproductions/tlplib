using pzd.lib.concurrent;
using pzd.lib.test_framework;
using NUnit.Framework;
using pzd.lib.exts;
using pzd.lib.functional;
using pzd.lib.test_framework.spec;

namespace com.tinylabproductions.TLPLib.Functional {
  public class NotReallyLazyTestAsFuture {
    const string value = "foo";
    static NotReallyLazyVal<string> create() => new NotReallyLazyVal<string>(value);

    [Test]
    public void ItShouldCompleteBeforeWeGetTheValue() {
      var lazy = create();
      var ftr = lazy.asFuture();
      ftr.isCompleted.shouldBeTrue();
      lazy.strict.forSideEffects();
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
      invoked.shouldEqual(1u, $"it should immediately invoke onComplete");
      lazy.strict.forSideEffects();
      invoked.shouldEqual(1u, $"it should not invoke onComplete twice");
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
      lazy.strict.forSideEffects();
      ftr.isCompleted.shouldBeTrue();
    }

    [Test]
    public void ItShouldHaveTheSameValueAsFuture() {
      var lazy = create();
      var ftr = lazy.asFuture();
      lazy.strict.forSideEffects();
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
      invoked.shouldEqual(0u, $"it should not invoke onComplete before .get.forSideEffects()");
      lazy.strict.forSideEffects();
      invoked.shouldEqual(1u, $"it should invoke onComplete after .get.forSideEffects()");
      lazy.strict.forSideEffects();
      invoked.shouldEqual(1u, $"it should not invoke onComplete twice");
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
          lazy.value.strict.forSideEffects();
          upcasted.value.isCompleted.shouldBeTrue();
        };
      };

      when["#" + nameof(lazy.value.onComplete)] = () => {
        var result = @let(Option<Base>.None);
        beforeEach += () => upcasted.value.onComplete(b => result.value = b.some());

        it["should transmit non-completion"] = () => result.value.shouldBeNone();
        it["should transmit completion"] = () => {
          lazy.value.strict.forSideEffects();
          result.value.shouldBeSome(obj);
        };
      };

      when["#" + nameof(lazy.value.value)] = () => {
        it["should transmit non-completion"] = () => upcasted.value.value.shouldBeNone();
        it["should transmit completion"] = () => {
          lazy.value.strict.forSideEffects();
          upcasted.value.value.shouldBeSome(obj);
        };
      };
    });
  }
}
