using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
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

  public class OptionTestOrElseFunction {
    [Test]
    public void WhenSome() {
      var option = new Option<string>("this is a set option");
      option.orElse(() => new Option<string>("if it was not set now it is")).shouldEqual(option);
    }
    [Test]
    public void WhenNone() {
      var option = new Option<string>();
      var newOption = new Option<string>("if it was not set now it is");
      option.orElse(() => newOption).shouldEqual(newOption);
    }
  }

  public class OptionTestOrElseValue {
    [Test]
    public void WhenSome() {
      var option = new Option<string>("this is a set option");
      option.orElse(new Option<string>("if it was not set now it is")).shouldEqual(option);
    }
    [Test]
    public void WhenNone() {
      var option = new Option<string>();
      var newOption = new Option<string>("if it was not set now it is");
      option.orElse(newOption).shouldEqual(newOption);
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
}