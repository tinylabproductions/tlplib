using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Data {
  public class DurationTestCreation {
    [Test]
    public void Constructor() {
      var d = new Duration(1400);
      d.millis.shouldEqual(1400);
      d.seconds.shouldEqual(1.4f);
    }

    [Test]
    public void FromSeconds() {
      var d = Duration.fromSeconds(1.4f);
      d.millis.shouldEqual(1400);
      d.seconds.shouldEqual(1.4f);
    }
  }

  public class DurationTestArithmetic {
    [Test]
    public void Addition() => 
      (new Duration(150) + new Duration(300)).shouldEqual(new Duration(450));

    [Test]
    public void Subtraction() {
      (new Duration(450) - new Duration(300)).shouldEqual(new Duration(150));
      (new Duration(150) - new Duration(300)).shouldEqual(new Duration(-150));
    }

    [Test]
    public void Multiplication() {
      (new Duration(100) * 3).shouldEqual(new Duration(300));
      (new Duration(100) * -3).shouldEqual(new Duration(-300));
    }

    [Test]
    public void Division() {
      (new Duration(120) / 3).shouldEqual(new Duration(40));
      (new Duration(-120) / 3).shouldEqual(new Duration(-40));
      (new Duration(100) / 3).shouldEqual(new Duration(33));
      (new Duration(101) / 3).shouldEqual(new Duration(33));
      (new Duration(102) / 3).shouldEqual(new Duration(34));
    }

    [Test]
    public void Less() {
      (new Duration(100) < new Duration(200)).shouldBeTrue();
      (new Duration(200) < new Duration(100)).shouldBeFalse();
    }

    [Test]
    public void LessEquals() {
      (new Duration(100) <= new Duration(200)).shouldBeTrue();
      // ReSharper disable once EqualExpressionComparison
      (new Duration(200) <= new Duration(200)).shouldBeTrue();
      (new Duration(200) <= new Duration(100)).shouldBeFalse();
    }

    [Test]
    public void More() {
      (new Duration(100) > new Duration(200)).shouldBeFalse();
      (new Duration(200) > new Duration(100)).shouldBeTrue();
    }

    [Test]
    public void MoreEquals() {
      (new Duration(100) >= new Duration(200)).shouldBeFalse();
      (new Duration(200) >= new Duration(100)).shouldBeTrue();
      // ReSharper disable once EqualExpressionComparison
      (new Duration(200) >= new Duration(200)).shouldBeTrue();
    }
  }
}