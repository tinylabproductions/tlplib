using System;
using pzd.lib.test_framework;
using NUnit.Framework;
using pzd.lib.exts;
using pzd.lib.functional;

namespace com.tinylabproductions.TLPLib.Functional {
  public class TestException : Exception {}

  public abstract class TryTestBase {
    public static readonly TestException ex = new TestException();
  }

  public class TryTestMap : TryTestBase {
    [Test] public void ErrorToGood() =>
      Try<int>.failed(ex).map(a => a * 2).shouldBeError(ex.GetType());

    [Test] public void ErrorToError() =>
      Try<int>.failed(ex).map<int, int>(a => throw new Exception()).shouldBeError(ex.GetType());

    [Test] public void GoodToGood() =>
      Try.value(1).map(a => a * 2).shouldBeSuccess(2);

    [Test] public void GoodToError() =>
      Try.value(1).map<int, int>(a => throw ex).shouldBeError(ex.GetType());
  }

  public class TryTestFlatMap : TryTestBase {
    static readonly ArgumentException ex2 = new ArgumentException("arg ex");

    [Test] public void ErrorToGood() =>
      Try<int>.failed(ex).flatMap(a => Try.value(a.ToString())).shouldBeError(ex.GetType());

    [Test] public void ErrorToError() =>
      Try<int>.failed(ex).flatMap(a => Try<string>.failed(ex2)).shouldBeError(ex.GetType());

    [Test] public void ErrorToExceptionInMapper() =>
      Try<int>.failed(ex).flatMap<int, string>(a => throw ex2).shouldBeError(ex.GetType());

    [Test] public void GoodToGood() =>
      Try<int>.value(1).flatMap(i => Try.value(i.ToString())).shouldBeSuccess("1");

    [Test] public void GoodToError() =>
      Try<int>.value(1).flatMap(i => Try<string>.failed(ex2)).shouldBeError(ex2.GetType());

    [Test] public void GoodToExceptionInMapper() =>
      Try<int>.value(1).flatMap<int, string>(i => throw ex2).shouldBeError(ex2.GetType());
  }
}
