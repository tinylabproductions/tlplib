using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public class SingletonActionRegistryTest : ImplicitSpecification {
    [Test]
    public void specification() => describe(() => {
      var registry = new SingletonActionRegistry<int>();

      when["future is going to complete in the future"] = () => {
        var f1 = let(() => F.lazy(() => 10));

        it["should not call the action until future is completed"] = () => {
          var called = false;
          registry[f1.value] = _ => called = true;
          called.shouldBeFalse();
        };

        it["should only call the last registered action"] = () => {
          var result = 0;
          registry[f1.value] = x => result = x;
          registry[f1.value] = x => result = x * 2;
          f1.value.get.forSideEffects();
          result.shouldEqual(f1.value.get * 2);
        };

        it["should not interfere between registered futures of same type"] = () => {
          var f2 = F.lazy(() => 20);
          var result1 = 0;
          var result2 = 0;
          registry[f1.value] = x => result1 = 0;
          registry[f1.value] = x => result1 = x;
          registry[f2] = x => result2 = 0;
          registry[f2] = x => result2 = x;
          f1.value.get.forSideEffects();
          f2.get.forSideEffects();
          F.t(result1, result2).shouldEqual(F.t(f1.value.get, f2.get));
        };
      };

      when["future is already completed"] = () => {
        const int VALUE = 10;
        var f1 = F.lazyLift(VALUE);

        it["should immediatelly call the given action every time it is called"] = () => {
          var result = 0;
          registry[f1] = x => result = x;
          result.shouldEqual(VALUE, "1st time");
          registry[f1] = x => result = x * 2;
          result.shouldEqual(VALUE * 2, "2nd time");
        };
      };
    });
  }
}