using System.Collections.Immutable;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class OptionTestFoldWithInitial {
    [Test]
    public void WhenNone() => F.none<string>().fold(5, (_, i) => i + 10).shouldEqual(5);

    [Test]
    public void WhenSome() => F.some(10).fold(5, (a, b) => a + b).shouldEqual(15);
  }

  public class OptionTestEquality {
    [Test]
    public void WhenEqualNone() {
      F.none<int>().shouldEqual(Option<int>.None);
      F.none<string>().shouldEqual(Option<string>.None);
    }

    [Test]
    public void WhenEqualSome() {
      F.some(3).shouldEqual(F.some(3));
      F.some(5).shouldEqual(F.some(5));
      F.some("foo").shouldEqual(F.some("foo"));
    }

    [Test]
    public void WhenNotEqual() {
      ImmutableList.Create(
        F.none<int>(),
        F.some(0),
        F.some(1)
      ).shouldTestInequalityAgainst(ImmutableList.Create(
        F.some(10), 
        F.some(11)
      ));
    }
  }

  public class OptionTestAsEnumDowncast {
    class A {}

    class B : A {}

    [Test]
    public void WhenSome() {
      var b = new B();
      new Option<B>(b).asEnum<A, B>().shouldEqual(b.Yield<A>());
    }

    [Test]
    public void WhenNone() {
      new Option<B>().asEnum<A, B>().shouldEqual(Enumerable.Empty<A>());
    }
  }

  public class OptionTestAsEnum {
    [Test]
    public void WhenSome() {
      F.some(3).asEnum().shouldEqual(3.Yield());
    }

    [Test]
    public void WhenOnlyA() {
      F.none<int>().asEnum().shouldEqual(Enumerable.Empty<int>());
    }
  }

  public class OptionTestCreateOrTap {
    [Test]
    public void WhenSome() {
      new Option<string>("stuff").createOrTap(() => "new stuff", p1 => p1.shouldEqual("stuff")).
        shouldBeSome("stuff");
    }

    [Test]
    public void WhenNone() {
      var ran = false;
      new Option<string>().createOrTap(() => "new stuff", p1 => ran = true).
        shouldBeSome("new stuff");
      ran.shouldBeFalse();
    }
  }

  public class OptionTestFoldFunctionFunction {
    [Test]
    public void WhenSomeGood() {
      new Option<string>("this is a set option").
        fold(() => false, op => op == "this is a set option").
        shouldBeTrue();
    }

    [Test]
    public void WhenNone() {
      new Option<string>().
        fold(() => false, op => op == "this is a set option").
        shouldBeFalse();
    }

    [Test]
    public void WhenSomeDiffer() {
      new Option<string>("This is").
        fold(() => false, op => op == "this is a set option").
        shouldBeFalse();
    }
  }

  public class OptionTestFoldValueFunction {
    [Test]
    public void WhenSomeGood() {
      new Option<string>("this is a set option").
        fold(false, op => op == "this is a set option").
        shouldBeTrue();
    }

    [Test]
    public void WhenNone() {
      new Option<string>().
        fold(false, op => op == "this is a set option").
        shouldBeFalse();
    }

    [Test]
    public void WhenSomeDiffer() {
      new Option<string>("This is").
        fold(false, op => op == "this is a set option").
        shouldBeFalse();
    }
  }

  public class OptionTestFoldValueValue {
    [Test]
    public void WhenSomeGood() {
      new Option<string>("this is a set option").
        fold(false, true).
        shouldBeTrue();
    }

    [Test]
    public void WhenNone() {
      new Option<string>().
        fold(false, true).
        shouldBeFalse();
    }

    [Test]
    public void WhenSomeDiffer() {
      new Option<string>("This is").
        fold(false, true).
        shouldBeTrue();
    }
  }

  public class OptionTestGetOrElseFunction {
    [Test]
    public void WhenSome() {
      new Option<bool>(true).getOrElse(() => false).shouldBeTrue();
    }

    [Test]
    public void WhenNone() {
      new Option<bool>().getOrElse(() => false).shouldBeFalse();
    }
  }

  public class OptionTestGetOrElseValue {
    [Test]
    public void WhenSome() {
      new Option<bool>(true).getOrElse(false).shouldBeTrue();
    }

    [Test]
    public void WhenNone() {
      new Option<bool>().getOrElse(false).shouldBeFalse();
    }
  }

  public class OptionTestGetOrNull {
    [Test]
    public void WhenSome() {
      new Option<string>("stuff").getOrNull().shouldEqual("stuff");
    }
    [Test]
    public void WhenNone() {
      new Option<string>().getOrNull().shouldEqual(null);
    }
  }

  public class OptionTestOrNull {
    [Test]
    public void WhenSome() {
      new Option<string>("stuff").orNull().shouldEqual("stuff");
    }

    [Test]
    public void WhenNone() {
      new Option<string>().orNull().shouldEqual(null);
    }
  }

  public class OptionEnumeratorTest {
    [Test]
    public void WhenSome() {
      var test = new Option<int>(5);
      var ran = false;
      foreach (var number in test) {
        number.shouldEqual(5);
        ran = true;
      }
      ran.shouldBeTrue();
    }

    [Test]
    public void WhenNone() {
      var test = new Option<int>();
      var ran = false;
      foreach (var number in test) ran = true;
      ran.shouldBeFalse();
    }
  }

  public class OptionTestOperatorTrue {
    [Test]
    public void WhenTrue() {
      var wasTrue = false;
      if (F.some(3)) wasTrue = true;
      wasTrue.shouldBeTrue();
    }

    [Test]
    public void WhenFalse() {
      var wasTrue = false;
      if (F.none<int>()) wasTrue = true;
      wasTrue.shouldBeFalse();
    }
  }

  public class OptionTestOperatorOr {
    [Test]
    public void WhenSome() {
      var option = new Option<string>("this is a set option");
      (option || new Option<string>("if it was not set now it is")).shouldEqual(option);
    }

    [Test]
    public void WhenNone() {
      var option = new Option<string>();
      var newOption = new Option<string>("if it was not set now it is");
      (option || newOption).shouldEqual(newOption);
    }
  }

  public class OptionTestSwap {
    [Test] public void WhenSome() => F.some(1).swap('a').shouldBeNone();
    [Test] public void WhenNone() => F.none<int>().swap('a').shouldBeSome('a');
    [Test] public void WhenNoneFn() => F.none<int>().swap(() => 'a').shouldBeSome('a');

    [Test]
    public void WhenSomeFn() {
      var called = false;
      F.some(1).swap(() => {
        called = true;
        return 'a';
      }).shouldBeNone();
      called.shouldBeFalse();
    }
  }
}