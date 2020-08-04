using NUnit.Framework;
using pzd.lib.test_framework;
using pzd.lib.test_framework.spec;

namespace com.tinylabproductions.TLPLib.Collection {
  public class Enumerable2Test : ImplicitSpecification {
    [Test]
    public void distribution() => describe(() => {
      it["should not distribute if there are no slots"] =
        () => Enumerable2.distribution(1000, 0).shouldBeEmpty();
      
      it["should not give anything if total is empty"] =
        () => Enumerable2.distribution(0, 5).shouldEqualEnum(new[] {0, 0, 0, 0, 0});
      
      it["should give first ones if there is not enough for all"] =
        () => Enumerable2.distribution(3, 5).shouldEqualEnum(new[] {1, 1, 1, 0, 0});
      
      it["should divide equally if possible"] =
        () => Enumerable2.distribution(10, 5).shouldEqualEnum(new[] {2, 2, 2, 2, 2});
      
      it["should give first ones the remainders"] =
        () => Enumerable2.distribution(13, 5).shouldEqualEnum(new[] {3, 3, 3, 2, 2});
    });
  }
}