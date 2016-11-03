using System;
using com.tinylabproductions.TLPLib.Test;
using NUnit.Framework;

namespace com.tinylabproductions.TLPLib.Functional {
  public class TestException : Exception {}

  public abstract class TryTestBase {
    public static readonly TestException ex = new TestException();
  }

  public class TryTestMap : TryTestBase {
    [Test] public void ErrorToGood() => 
      new Try<int>(ex).map(a => a * 2).shouldBeError(ex.GetType());

    [Test] public void ErrorToError() => 
      new Try<int>(ex).map<int>(a => { throw new Exception(); }).shouldBeError(ex.GetType());

    [Test] public void GoodToGood() => 
      new Try<int>(1).map(a => a * 2).shouldBeSuccess(2);

    [Test] public void GoodToError() => 
      new Try<int>(1).map<int>(a => { throw ex; }).shouldBeError(ex.GetType());
  }

  public class TryTestFlatMap : TryTestBase {
    static readonly ArgumentException ex2 = new ArgumentException("arg ex");

    [Test] public void ErrorToGood() =>
      new Try<int>(ex).flatMap(a => new Try<string>(a.ToString())).shouldBeError(ex.GetType());

    [Test] public void ErrorToError() =>
      new Try<int>(ex).flatMap(a => new Try<string>(ex2)).shouldBeError(ex.GetType());

    [Test] public void ErrorToExceptionInMapper() =>
      new Try<int>(ex).flatMap<string>(a => { throw ex2; }).shouldBeError(ex.GetType());

    [Test] public void GoodToGood() =>
      new Try<int>(1).flatMap(i => new Try<string>(i.ToString())).shouldBeSuccess("1");

    [Test] public void GoodToError() =>
      new Try<int>(1).flatMap(i => new Try<string>(ex2)).shouldBeError(ex2.GetType());

    [Test] public void GoodToExceptionInMapper() =>
      new Try<int>(1).flatMap<string>(i => { throw ex2; }).shouldBeError(ex2.GetType());
  }
}
